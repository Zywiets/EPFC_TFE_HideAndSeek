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
var hsPlayers = [];


app.get('/', function(req, res){
    res.send("Server is running!");
});

function HashPassword(password) {
    const hash = crypto.createHash('sha256');
    hash.update(password);
    return hash.digest('hex');
}

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
               }else{
                   socket.emit('sign in', false)
               }

            });
        });

        socket.on('sign up', data => {
            sqlCon.query("SELECT COUNT(*) as total FROM users WHERE username = ? OR ?",[data.username, data.email], function(err, result, fields){
                if(err) throw err;
                let tot = parseInt(result[0].total);
                console.log("***** le total est "+ tot+" ******");
                if(tot > 0 ) {
                    socket.emit('sign in', false);
                }else{
                    const newUser = {
                        username : data.username,
                        email : data.email,
                        password : HashPassword(data.password)
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
                        socket.emit('sign up', result);
                    })
                }
            })
        })

        socket.on('play', data => {
            console.log('Player started playing: ', socket.id);

            if (clients.length === 0) {
                playerSpawnPoints = [];
                data.playerSpawnPoints.forEach(function (_playerSpawnPoint) {
                    var playerSpawnPoint = {
                        position: _playerSpawnPoint.position,
                        rotation: _playerSpawnPoint.rotation
                    };
                    playerSpawnPoints.push(playerSpawnPoint);
                });
            }
            var randomSpawnPoint = playerSpawnPoints[Math.floor(Math.random() * playerSpawnPoints.length)];
            currentPlayer = {   name: data.name,
                                position: randomSpawnPoint.position ? randomSpawnPoint.position : { x: 0, y: 0, z: 0 },
                                rotation: randomSpawnPoint.rotation ? randomSpawnPoint.rotation : { x: 0, y: 0, z: 0 },
                                movement: 0
            };
            clients.push(currentPlayer);
            // in your game, tells you that you have joined
            console.log(currentPlayer.name + ' emit: play: ' + JSON.stringify(currentPlayer));
            socket.emit('play', currentPlayer);
            socket.broadcast.emit('other player connected', currentPlayer);
        });

        socket.on('player move', function (data) {
            // console.log('received: move: ' + JSON.stringify(data));
            currentPlayer.movement = data.movement;
            currentPlayer.rotation = data.rotation;
            currentPlayer.position = data.position;
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