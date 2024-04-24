DROP DATABASE IF EXISTS `hs-db`;
CREATE DATABASE IF NOT EXISTS `hs-db` DEFAULT CHARACTER SET utf8 COLLATE utf8_general_ci;
USE `hs-db`;

CREATE TABLE users (
    user_id INT AUTO_INCREMENT PRIMARY KEY,
    username VARCHAR(255) NOT NULL,
    email VARCHAR(255) NOT NULL,
    password VARCHAR(255) NOT NULL
);

CREATE TABLE game_data (
    game_data_id INT AUTO_INCREMENT PRIMARY KEY,
    user_id INT NOT NULL,
    player_score INT NOT NULL,
    FOREIGN KEY (user_id) REFERENCES users(user_id)
);

INSERT INTO users (username, email, password) VALUES
('admin','admin@admin.com','admin'),
('test','test@test.com','test'),
('MLGProSniper', 'ItadoriTimothée12@gmail.com', 'Password1+'),
('ToxicTrollKing', 'MichelPlumart@hotmail.com', 'Password2+'),
('xXDarkLord70Xx', 'ShakiraFan1984@star-co.net.kp', 'TrueLeader4+');

INSERT INTO game_data (user_id, player_score) VALUES
(1, 0),
(2,0),
(3, 1500),
(4, 750),
(5, 750);

CREATE TRIGGER after_user_insert
AFTER INSERT ON users
FOR EACH ROW
BEGIN
    INSERT INTO game_data (user_id, player_score) VALUES (NEW.user_id, 0);
END;