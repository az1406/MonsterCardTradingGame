DROP TABLE IF EXISTS users, cards, stack, deck;

CREATE TABLE IF NOT EXISTS users (
                                     id SERIAL PRIMARY KEY,
                                     username VARCHAR(255) NOT NULL UNIQUE,
                                     password VARCHAR(255) NOT NULL,
                                     token VARCHAR(255),
                                     bio TEXT,
                                     image TEXT,
                                     elo INT,
                                     coins INT,
                                     games_played INT
);

CREATE TABLE IF NOT EXISTS cards (
                                     id VARCHAR(255) PRIMARY KEY,
                                     name VARCHAR(255) NOT NULL,
                                     element_type VARCHAR(255) NOT NULL,
                                     package_number INT NOT NULL,
                                     is_spell BOOLEAN NOT NULL,
                                     damage DOUBLE PRECISION NOT NULL
);

CREATE TABLE IF NOT EXISTS stack (
                                     user_token VARCHAR(255) NOT NULL,
                                     card_id VARCHAR(255) NOT NULL,
                                     package_number INT NOT NULL,
                                     PRIMARY KEY (user_token, card_id)
);

CREATE TABLE IF NOT EXISTS deck (
                                    user_token VARCHAR(255) NOT NULL,
                                    card_id VARCHAR(255) NOT NULL,
                                    PRIMARY KEY (user_token, card_id)
);