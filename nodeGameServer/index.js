const app = require('express')();
const server = require('http').createServer(app);
const io = require('socket.io')(server);
const crypto = require('crypto');
const mysql = require('mysql');
const {TIME} = require("mysql/lib/protocol/constants/types");
const sqlCon = mysql.createConnection({
    host: "127.0.0.1",
    user: "root",
    password: "7412",
    database: "hs-db"
});
server.listen(3000);

var lobbies = [];


app.get('/', function (req, res) {
    res.send("Server is running!");
});

io.on('connection', function (socket) {
    console.log(`New connection: ${socket.id}`);

    socket.on('createLobby', user => {
        console.log("new lobby host", user)
        const lobby = {name: user.username, id: user.socketId, users: [user]}
        lobbies.push(lobby)
        io.emit('lobbyCreated', lobby)
        console.log("createLobby", lobby)
        socket.emit('lobbyJoined', lobby)
        socket.join(lobby.id)
    })

    socket.on('joinLobby', (data) => {
        console.log("lobby chosen", data);
        let lobbyId = data.lobbyId;
        if (!lobbyId) {
            return;
        }
        if (!lobbies[lobbyId]) {
            lobbies[lobbyId] = []; // Ensure the lobby array exists
        }
        const l = lobbies.find((lobby) => lobby.id === lobbyId);
        l.users.push(data);
        socket.join(lobbyId)
        io.to(lobbyId).emit('lobbyJoined', l);
    });

    socket.on('lobbyDelete', (user) => {
        console.log("delete lobby", user);
        lobbies = lobbies.filter((lobby) => lobby.id !== user.lobbyId);
        io.emit('lobbyDeleted', user.lobbyId);
    })


    socket.on('start', lobby => {
        console.log('Player started playing: ', lobby);
        for (let user of lobby.users) {
            user.point = lobby.spawnPoints.pop()
            console.log(user.point)
        }
        io.to(lobby.id).emit('roundStarted', lobby)
    });

    socket.on('saveScores', scores => {
        console.log("add scores", scores);
        const highestScore = Math.max(...scores.map(score => score.total));
        const winnerIds = scores.filter(score => score.total === highestScore).map(score => score.userId);
        let gameId = ""
        sqlCon.query('INSERT INTO games (game_date) VALUES (NOW())', function (err, result, fields) {
            if (err) throw err;
            gameId = result.insertId;
            for (const score of scores) {
                const userId = score.userId;
                const total = score.total;
                sqlCon.query('INSERT INTO score (user_id, game_id, points) VALUES (?, ?, ?)', [userId, gameId, total]);
            }

            for (const winnerId of winnerIds) {
                sqlCon.query('UPDATE score SET is_winner = TRUE WHERE game_id = ? AND user_id = ?', [gameId, winnerId]);
            }
        });
    })

    socket.on('gameEnd', data => {
        console.log("gameEnded", data)
        const lobbyId = data.lobbyId;
        lobbies = lobbies.filter(lobby => lobby.id !== lobbyId);
        io.to(lobbyId).emit('gameEnded');
        io.in(lobbyId).socketsLeave(lobbyId);
    })

    // TODO: wrong username input triggers exception
    socket.on('login', data => {
        console.log('login: ', data);
        sqlCon.query("SELECT users.password FROM users WHERE username = ?", [data.username], function (err, res, fields) {
            if (err) throw err;
            const hashPass = data.password;
            if (res[0].password === hashPass) {
                socket.emit('sign in', true)
                sqlCon.query("SELECT user_id FROM users WHERE username = ?", [data.username], function (err, resu, fields) {
                    if (err) throw err
                    const userId = resu[0].user_id;
                    socket.emit("loginSucceeded", {
                        id: userId,
                        socketId: socket.id,
                    })
                })
            } else {
                socket.emit('loginFailed')
            }

        });
    });

    socket.on('register', data => {
        console.log("sign up", data)
        sqlCon.query("SELECT COUNT(*) as total FROM users WHERE username = ? OR ?", [data.username, data.email], function (err, result, fields) {
            if (err) throw err;
            let tot = parseInt(result[0].total);
            if (tot > 0) {
                socket.emit('loginFailed');
                return
            }

            var user = {
                email: data.email,
                username: data.username,
                password: data.password,
            }

            sqlCon.query("INSERT INTO `users` SET ?", user, function (err, result, fields) {
                if (err) throw err;
                if (result && result.affectedRows > 0) {
                    sqlCon.query("SELECT user_id FROM users WHERE username = ?", [data.username], function (err, result, fields) {
                        socket.emit("registerSucceeded", {
                            id: result[0].user_id,
                            socketId: socket.id,
                            username: data.username,
                            email: data.email,
                        })
                    })
                    return
                }
                socket.emit('registerFailed', result);
            })
        })
    })

    socket.on('rankings', data => {
        console.log("rankings", data)
        sqlCon.query("SELECT u.user_id, u.username, COALESCE(SUM(s.points), 0) AS total_score FROM users u LEFT JOIN  score s ON u.user_id = s.user_id GROUP BY  u.user_id ORDER BY total_score DESC", function (err, result, fields) {
            if (err) throw err;
            let userRankings = []
            result.forEach(function (data) {
                let userRank = {
                    username: data.username,
                    total: data.total_score
                }
                userRankings.push(userRank);
            })
            socket.emit('rankingsReceived', {rankings: userRankings})
        })
    })

    socket.on('seekingStart', function (user) {
        // console.log("started seeking", user);
        io.to(user.lobbyId).emit('seekingStarted')
    })

    socket.on('userFound', function (user) {
        console.log("has been found", user);
        io.to(user.lobbyId).emit('userFound', user)
    })

    socket.on('gameEnd', function (data) {
        console.log("gameEnd", data)
        io.to(data.lobbyId).emit("game ended")
    })

    socket.on('scoreAdd', function (user) {
        console.log("final score", user);
        io.to(user.lobbyId).emit('scoreAdded', user)
    })

    socket.on('userMove', function (data) {
        io.to(data.lobbyId).emit('userMoved', data);
    });


    socket.on('disconnect', function () {
        console.log(`disconnect: ${socket.id}`);
        const filtered_lobbies = lobbies.filter(lobby => {
            return lobby.users.some(user => user.socketId === socket.id);
        });

        console.log("filtered_lobbies", filtered_lobbies);
        console.log(filtered_lobbies.length)

        if (filtered_lobbies.length !== 1) return
        const lobby = filtered_lobbies[0];
        const isLobbyHost = lobby.id === socket.id
        lobby.users = lobby.users.filter(user => user.socketId !== socket.id);
        io.to(lobby.id).emit('disconnected', {
            lobby: lobby,
            isLobbyHost: isLobbyHost,
        });
        console.log("lobby", lobby);
        console.log("isLobbyHost", isLobbyHost)
        if (isLobbyHost) {
            lobbies = lobbies.filter(lobby => lobby.id !== socket.id);
            console.log("host disconnected biatch")
            io.emit('hostDisconnected', socket.id)
        }
    });
});

console.log('----Server is Running----');