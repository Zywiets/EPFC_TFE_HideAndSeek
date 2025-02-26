const app = require('express')();
const server = require('http').createServer(app);
const io = require('socket.io')(server);
const crypto = require('crypto');
const mysql = require('mysql');
const {TIME} = require("mysql/lib/protocol/constants/types");
const sqlCon = mysql.createConnection({
    host: "localhost",
    user: "root",
    password: "",
    database: "hs-db"
});
server.listen(3000);

var playerSpawnPoints = [];
var clients = [];
var lobby = [];
var lobbies = {};
let hosts = [];
var hsPlayers = [];


app.get('/', function(req, res){
    res.send("Server is running!");
});

io.on('connection', function (socket) {
        console.log(`new connection socket: ${socket.id}`);

        var currentPlayer = {};

        socket.on('player connect', () => {
            console.log('Player connected:', socket.id);
            if(clients.length > 0) {
                clients.forEach((client) => {
                    const playerConnected = {
                        name: client.name,
                        position: client.position,
                        rotation: client.rotation
                    };
                    socket.emit('other player connected', playerConnected);
                });
            }
        });

        socket.on('get lobbies', data => {
            socket.emit('hosts data', hosts);
        })

        socket.on('new lobby host', data => {
            const lobbyId = data.lobby
            const player = { lobby: lobbyId, name: data.name };
            if (!Array.isArray(lobbies[lobbyId])) {
                lobbies[lobbyId] = [];
            }
            lobbies[lobbyId].push(player);
            hosts.push(lobbyId);
            socket.broadcast.emit('new host', data);
            socket.join(lobbyId);
        })

        socket.on('lobby chosen', (data) => {
            let lobbyId = data.lobby;
            if (!lobbyId) {
                return;
            }
            if (!lobbies[lobbyId]) {
                lobbies[lobbyId] = []; // Ensure the lobby array exists
            }
            const player = { lobby: lobbyId, name: data.name };
            socket.emit('others in lobby', lobbies[lobbyId]);
            lobbies[lobbyId].push(player);
            socket.join(lobbyId)
            socket.to(lobbyId).emit('other player in lobby', player);
        });

        socket.on('join lobby', data => {
            console.log('connect to lobby')

            if(lobby.length > 0) {
                lobby.forEach((user) =>{
                    const lobbyPlayer = {
                        name: user.name
                    }
                    socket.emit('other player in lobby', lobbyPlayer)
                })
            }else {
                socket.emit("lobby host")
            }
            let user = { name: data.name}
            lobby.push(user);
            socket.broadcast.emit('other player in lobby', user)
        })

        socket.on('create lobby', data => {
            console.log('create lobby');
            // create the new room and add the user
            socket.join(data)
        })


        socket.on('play', data => {
            console.log('Player started playing: ', socket.id);
            const lobbyId = data.lobby
            playerSpawnPoints = [];
            data.playerSpawnPoints.forEach(function (_playerSpawnPoint) {
                let playerSpawnPoint = {
                    position: _playerSpawnPoint.position,
                    rotation: _playerSpawnPoint.rotation
                };
                playerSpawnPoints.push(playerSpawnPoint);
            });
            let ReadyToPlayers = [];
            for (let i = 0; i < lobbies[lobbyId].length; i++) {
                let randomSpawnPoint = playerSpawnPoints[i]; // Now randomized
                let lobPla = lobbies[lobbyId][i];
                let play = {
                    name: lobPla.name,
                    position: randomSpawnPoint.position,
                    rotation: randomSpawnPoint.rotation,
                    movement: 0,
                };
                ReadyToPlayers.push(play);
                console.log(play)
            }
            console.log(ReadyToPlayers)
            socket.emit('play', ReadyToPlayers)
            //socket.broadcast.emit('play', ReadyToPlayers)
            socket.to(data.lobby).emit('play', ReadyToPlayers)
        });

        socket.on('empty clients', data =>{
            const highestScore = Math.max(...data.map(player => player.score));
            const winners = data.filter(player => player.score === highestScore).map(player => player.id);
            let gameId = ""
            sqlCon.query('INSERT INTO games (game_date) VALUES (NOW())', function(err, result, fields){
                if (err) throw err;
                gameId = result.insertId;
                console.log('le gameId est '+gameId)
                for (const player of data) {
                    const userId = player.id;
                    const score = player.score;
                    sqlCon.query('INSERT INTO score (user_id, game_id, points) VALUES (?, ?, ?)', [userId, gameId, score]);
                }

                for (const winnerId of winners) {
                    sqlCon.query('UPDATE score SET is_winner = TRUE WHERE game_id = ? AND user_id = ?', [gameId, winnerId]);
                }
            });



            lobby = [];
            console.log("emptying lobby list")
        })

        socket.on('test', data =>{
            sqlCon.query("SELECT * FROM users", function(err, result, fields){
                if (err) throw err;
                result.forEach(row => {
                    hsPlayers.push(row);
                })
                const testResponse = {
                    users: hsPlayers
                };
                socket.emit('test', testResponse);
            });
        });

        socket.on('sign in', data => {
            console.log('Player trying to sign in: ',data);
            sqlCon.query("SELECT users.password FROM users WHERE username = ?", [data.username], function(err, res, fields){
               if(err) throw err;
               const hashPass = data.password;
               console.log('le mdp du user :', data.password)
                console.log("le mdp de la DB", res[0].password)
               if(res[0].password === hashPass){
                   socket.emit('sign in', true)
                   sqlCon.query("SELECT user_id FROM users WHERE username = ?", [data.username], function(err, resu, fields){
                       if(err) throw err
                       const userId = resu[0].user_id;
                       socket.emit('user_id', userId)
                   })
               }else{
                   socket.emit('sign in', false)
               }

            });
        });

        socket.on('sign up', data => {
            console.log("On passe dans le sign up 1")
            sqlCon.query("SELECT COUNT(*) as total FROM users WHERE username = ? OR ?",[data.username, data.email], function(err, result, fields){
                if(err) throw err;
                let tot = parseInt(result[0].total);
                if(tot > 0 ) {
                    socket.emit('sign in', false);
                }else{
                    console.log("On passe dans le sign up 2")
                    const newUser = {
                        username : data.username,
                        email : data.email,
                        password : data.password
                    }
                    sqlCon.query("INSERT INTO `users` SET ?", newUser, function(err, result, fields){
                        if(err) throw err;
                        if (result && result.affectedRows > 0) {
                            socket.emit("sign up", true)
                            console.log('Query executed successfully. Affected rows:', result.affectedRows);
                        } else {
                            socket.emit("sign up" , false)
                            console.log('Query executed but did not affect any rows.');
                        }
                        //socket.emit('sign up', result);
                    })
                }
            })
        })

        socket.on('rankings', data => {
            sqlCon.query("SELECT u.user_id, u.username, COALESCE(SUM(s.points), 0) AS total_score FROM users u LEFT JOIN  score s ON u.user_id = s.user_id GROUP BY  u.user_id ORDER BY total_score DESC", function(err, result, fields){
                if(err) throw err;
                let userRankings= []
                result.forEach(function(data) {
                    let userRank = {
                        username : data.username,
                        totalScore : data.total_score
                    }
                    userRankings.push(userRank);
                })
                socket.emit('rankings', userRankings)
            })
        })

        socket.on('first seeker', function(data){
            console.log('The first seeker is being set : '+ data)
            socket.broadcast.emit('first seeker')
        })
        socket.on('started seeking' ,function(data){
            socket.broadcast.emit('started seeking')
        })

        socket.on('has been found', function (data){
            socket.broadcast.emit('player found', data)
        })
        socket.on('round timer over', function(data){
            socket.broadcast.emit('round over')
        })
        socket.on('final score', function(data){
            let finalScore  = { name : data.name,
            score : data.score}
            console.log('le score broadcast est '+data.name+' '+data.score)
            socket.broadcast.emit('player score', data)
        })

        socket.on('player move', function (data) {
            //console.log('received: move: ' + JSON.stringify(data));
            currentPlayer.name = data.name;
            currentPlayer.movement = data.movement;
            currentPlayer.rotation = data.rotation;
            currentPlayer.position = data.position;
            //socket.to(lobbyId).emit('player move', currentPlayer);
            socket.broadcast.emit('player move', currentPlayer);
        });

        socket.on('player jump', function(data) {
           socket.broadcast.emit('player jump', data);
        });

        socket.on('player turn', function (data) {
            console.log('received: turn: ' + JSON.stringify(data));
            currentPlayer.rotation = data.position;
            socket.broadcast.emit('player turn', currentPlayer);
        });

        socket.on('disconnect', function () {
            console.log(currentPlayer.name + ' received: disconnect ' + JSON.stringify(currentPlayer));
            socket.broadcast.emit('other player disconnected', currentPlayer);
            console.log(currentPlayer.name + ' broadcast: other player disconnected ' + JSON.stringify(currentPlayer));
            //for loop to modify the data structure of clients
            for (let i = 0; i < clients.length; i++) {
                if (clients[i].name === currentPlayer.name) {
                    clients.splice(i, 1);
                }

            }
        });
    });

console.log('----Server is Running----');