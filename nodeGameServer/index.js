var app = require('express')();
var server = require('http').createServer(app);
var io = require('socket.io')(server);

server.listen(3000);

var playerSpawnPoints = [];
var clients = [];


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