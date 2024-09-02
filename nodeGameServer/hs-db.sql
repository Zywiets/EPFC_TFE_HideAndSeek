DROP DATABASE IF EXISTS `hs-db`;
CREATE DATABASE IF NOT EXISTS `hs-db` DEFAULT CHARACTER SET utf8 COLLATE utf8_general_ci;
USE `hs-db`;

CREATE TABLE users (
    user_id INT AUTO_INCREMENT PRIMARY KEY,
    username VARCHAR(255) NOT NULL,
    email VARCHAR(255) NOT NULL,
    password VARCHAR(255) NOT NULL
);

CREATE TABLE games (
    game_id INT AUTO_INCREMENT PRIMARY KEY,
    winner_id INT NOT NULL,
    FOREIGN KEY (winner_id) REFERENCES users(user_id)
);

CREATE TABLE score (
 user_id INT NOT NULL,
 game_id INT NOT NULL,
 points INT NOT NULL,
 FOREIGN KEY (user_id) REFERENCES users(user_id),
 FOREIGN KEY (game_id) REFERENCES games(game_id)
);

INSERT INTO users (username, email, password) VALUES
('admin','admin@admin.com','f3d5a893b56d7acdb3d92475406b622d'), /* admin*/
('test','test@test.com','04ee25ca867010281617898e30a90fbf'), /*test*/
('MLGProSniper', 'ItadoriTimothée12@gmail.com', '7390675469f427512e7fb25a0522711a'), /* Password1+ */
('ToxicTrollKing', 'MichelPlumart@hotmail.com', '9074f9fd5a8ed9f94cd764b9e405ac0b'), /* Password2+ */
('xXDarkLord70Xx', 'ShakiraFan1984@star-co.net.kp', '6fd4791f5ccce852f98ce3919feefcff'); /* TrueLeader4+ */

INSERT INTO games (winner_id) VALUES
(1),
(1);


INSERT INTO score (user_id, game_id, points) VALUES
(1, 1, 450),
(2, 1, 350),
(3, 1, 100),
(4, 1, 50),
(1, 2, 550),
(2, 2, 250),
(3, 2, 50),
(4, 2, 150);

