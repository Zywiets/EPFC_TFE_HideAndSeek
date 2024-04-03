var app = require('express')();
var server = require('http').createServer(app);
var io = require('socket.io')(server);

server.listen(3000);

var playerSpawnPoints = [];
var clients = [];


app.get('/', function(req, res){
    res.send("hey you answered my message");
});

io.on('connection',
    function (socket) {
        console.log("new connection socket" + socket.id);

        var currentPlayer = {};
        currentPlayer.name = 'unknown';

        socket.on('player connect', () => {
            console.log(currentPlayer.name + ' received. player connected');
            for (let i = 0; clients.length > i; i++) {
                var playerConnected = {
                    name: clients[i].name,
                    position: clients[i].position,
                    rotation: clients[i].rotation
                };
                //receive info about other players
                socket.emit('other player connected', playerConnected);
                console.log(currentPlayer.name + 'emit: other player connected: ' + JSON.stringify(playerConnected));

            }
        });

        socket.on('play', data => {
            console.log(currentPlayer.name + ' received: play: ' + JSON.stringify(data));

            if (0 === clients.length) {
                playerSpawnPoints = [];
                data.playerSpawnPoints.forEach(function (_playerSpawnPoint) {
                    var playerSpawnPoint = {
                        position: _playerSpawnPoint.position,
                        rotation: _playerSpawnPoint.rotation
                    };
                    playerSpawnPoints.push(playerSpawnPoint);
                });
            }
            console.log(currentPlayer.name + ' play: ' + JSON.stringify(currentPlayer));
            console.log("-----------"+playerSpawnPoints)
            var randomSpawnPoint = playerSpawnPoints[Math.floor(Math.random() * playerSpawnPoints.length)];
            currentPlayer = {   name: data.name,
                                position: randomSpawnPoint.position ? randomSpawnPoint.position : { x: 0, y: 0, z: 0 },
                                rotation: randomSpawnPoint.rotation ? randomSpawnPoint.rotation : { x: 0, y: 0, z: 0 },
                                movement: 0
            };
            clients.push(currentPlayer);
            // in your current game, tells you that you have joined
            console.log(currentPlayer.name + ' emit: play: ' + JSON.stringify(currentPlayer));
            socket.emit('play', currentPlayer);
            socket.broadcast.emit('other player connected', currentPlayer);
        });

        socket.on('player move', function (data) {
            // console.log('received: move: ' + JSON.stringify(data));
            currentPlayer.movement = data.movement;
            socket.broadcast.emit('player move', currentPlayer);
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