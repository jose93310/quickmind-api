-- QuickMind Database Schema (PostgreSQL)
-- Ejecutar en orden

-- 1. Tabla de usuarios
CREATE TABLE IF NOT EXISTS users (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    email VARCHAR(255) UNIQUE NOT NULL,
    nickname VARCHAR(50) NOT NULL,
    name VARCHAR(100),
    country VARCHAR(50),
    city VARCHAR(50),
    birth_date DATE,
    gender VARCHAR(20),
    avatar_path VARCHAR(500),
    password_hash VARCHAR(500) NOT NULL,
    is_guest BOOLEAN DEFAULT FALSE,
    created_at TIMESTAMP DEFAULT NOW()
);

-- 2. Tabla de partidas
CREATE TABLE IF NOT EXISTS games (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    code VARCHAR(6) UNIQUE NOT NULL,
    host_id UUID REFERENCES users(id),
    status INTEGER NOT NULL DEFAULT 0,
    max_players INTEGER DEFAULT 8,
    total_rounds INTEGER NOT NULL,
    current_round INTEGER DEFAULT 0,
    time_per_round INTEGER NOT NULL,
    letter_mode INTEGER NOT NULL,
    validation_type INTEGER NOT NULL,
    created_at TIMESTAMP DEFAULT NOW(),
    started_at TIMESTAMP,
    finished_at TIMESTAMP,
    is_public BOOLEAN DEFAULT FALSE,
    scheduled_start TIMESTAMP
);

-- 3. Tabla de jugadores en partida
CREATE TABLE IF NOT EXISTS players (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    game_id UUID REFERENCES games(id) ON DELETE CASCADE,
    user_id UUID REFERENCES users(id),
    nickname VARCHAR(50) NOT NULL,
    avatar_path VARCHAR(500),
    score INTEGER DEFAULT 0,
    is_host BOOLEAN DEFAULT FALSE,
    is_online BOOLEAN DEFAULT TRUE,
    joined_at TIMESTAMP DEFAULT NOW(),
    left_at TIMESTAMP
);

-- 4. Tabla de rondas
CREATE TABLE IF NOT EXISTS game_rounds (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    game_id UUID REFERENCES games(id) ON DELETE CASCADE,
    round_number INTEGER NOT NULL,
    letter VARCHAR(1) NOT NULL,
    started_at TIMESTAMP DEFAULT NOW(),
    ended_at TIMESTAMP,
    status INTEGER DEFAULT 0
);

-- 5. Tabla de respuestas
CREATE TABLE IF NOT EXISTS answers (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    round_id UUID REFERENCES game_rounds(id) ON DELETE CASCADE,
    player_id UUID REFERENCES players(id) ON DELETE CASCADE,
    category VARCHAR(50) NOT NULL,
    text VARCHAR(100) NOT NULL,
    is_valid BOOLEAN,
    points INTEGER DEFAULT 0,
    created_at TIMESTAMP DEFAULT NOW()
);

-- 6. Tabla de categorías
CREATE TABLE IF NOT EXISTS categories (
    id SERIAL PRIMARY KEY,
    name VARCHAR(50) NOT NULL,
    display_name VARCHAR(100) NOT NULL,
    age_group INTEGER NOT NULL,
    icon VARCHAR(50),
    is_active BOOLEAN DEFAULT TRUE
);

-- 7. Tabla de votos en rondas
CREATE TABLE IF NOT EXISTS round_votes (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    answer_id UUID REFERENCES answers(id) ON DELETE CASCADE,
    voter_id UUID REFERENCES players(id) ON DELETE CASCADE,
    is_valid BOOLEAN NOT NULL,
    created_at TIMESTAMP DEFAULT NOW(),
    UNIQUE(answer_id, voter_id)
);

-- 8. Tabla de solicitudes de amistad
CREATE TABLE IF NOT EXISTS friend_requests (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    sender_id UUID REFERENCES users(id) ON DELETE CASCADE,
    receiver_id UUID REFERENCES users(id) ON DELETE CASCADE,
    status INTEGER NOT NULL DEFAULT 0,
    created_at TIMESTAMP DEFAULT NOW(),
    UNIQUE(sender_id, receiver_id)
);

-- 9. Tabla de amigos (relación bidireccional)
CREATE TABLE IF NOT EXISTS friends (
    user_id UUID REFERENCES users(id) ON DELETE CASCADE,
    friend_id UUID REFERENCES users(id) ON DELETE CASCADE,
    created_at TIMESTAMP DEFAULT NOW(),
    PRIMARY KEY (user_id, friend_id)
);

-- 10. Tabla de mensajes de sala de juego
CREATE TABLE IF NOT EXISTS game_messages (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    game_id UUID REFERENCES games(id) ON DELETE CASCADE,
    sender_id UUID REFERENCES users(id) ON DELETE CASCADE,
    text TEXT,
    media_url TEXT,
    media_type VARCHAR(20),
    created_at TIMESTAMP DEFAULT NOW()
);

-- 11. Tabla de mensajes directos entre amigos
CREATE TABLE IF NOT EXISTS chat_messages (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    sender_id UUID REFERENCES users(id) ON DELETE CASCADE,
    receiver_id UUID REFERENCES users(id) ON DELETE CASCADE,
    text TEXT,
    media_url TEXT,
    media_type VARCHAR(20),
    is_read BOOLEAN DEFAULT FALSE,
    created_at TIMESTAMP DEFAULT NOW()
);

-- 12. Tabla de reacciones durante partida
CREATE TABLE IF NOT EXISTS game_reactions (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    game_id UUID REFERENCES games(id) ON DELETE CASCADE,
    player_id UUID REFERENCES users(id) ON DELETE CASCADE,
    reaction VARCHAR(50) NOT NULL,
    created_at TIMESTAMP DEFAULT NOW()
);

-- 13. Tabla de estadísticas de jugadores
CREATE TABLE IF NOT EXISTS player_stats (
    user_id UUID PRIMARY KEY REFERENCES users(id) ON DELETE CASCADE,
    games_played INTEGER DEFAULT 0,
    games_won INTEGER DEFAULT 0,
    games_lost INTEGER DEFAULT 0,
    total_score INTEGER DEFAULT 0,
    current_streak INTEGER DEFAULT 0,
    best_streak INTEGER DEFAULT 0,
    total_answers INTEGER DEFAULT 0,
    correct_answers INTEGER DEFAULT 0,
    last_played_at TIMESTAMP
);

-- 14. Tabla de logros
CREATE TABLE IF NOT EXISTS achievements (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name VARCHAR(100) NOT NULL,
    description VARCHAR(255),
    icon VARCHAR(50),
    points INTEGER DEFAULT 0,
    requirement_type VARCHAR(50),
    requirement_value INTEGER,
    created_at TIMESTAMP DEFAULT NOW()
);

-- 15. Tabla de logros obtenidos por usuario
CREATE TABLE IF NOT EXISTS user_achievements (
    user_id UUID REFERENCES users(id) ON DELETE CASCADE,
    achievement_id UUID REFERENCES achievements(id) ON DELETE CASCADE,
    earned_at TIMESTAMP DEFAULT NOW(),
    PRIMARY KEY (user_id, achievement_id)
);

-- Índices
CREATE INDEX IF NOT EXISTS idx_games_code ON games(code);
CREATE INDEX IF NOT EXISTS idx_games_public ON games(is_public, status, scheduled_start);
CREATE INDEX IF NOT EXISTS idx_players_game_id ON players(game_id);
CREATE INDEX IF NOT EXISTS idx_game_rounds_game_id ON game_rounds(game_id);
CREATE INDEX IF NOT EXISTS idx_answers_round_id ON answers(round_id);
CREATE INDEX IF NOT EXISTS idx_friends_user_id ON friends(user_id);
CREATE INDEX IF NOT EXISTS idx_friends_friend_id ON friends(friend_id);
CREATE INDEX IF NOT EXISTS idx_friend_requests_receiver ON friend_requests(receiver_id, status);
CREATE INDEX IF NOT EXISTS idx_users_nickname ON users USING gin(nickname gin_trgm_ops);
CREATE INDEX IF NOT EXISTS idx_game_messages_game_id ON game_messages(game_id, created_at);
CREATE INDEX IF NOT EXISTS idx_chat_messages_conversation ON chat_messages(sender_id, receiver_id, created_at);
CREATE INDEX IF NOT EXISTS idx_game_reactions_game ON game_reactions(game_id, created_at);

-- Seed de categorías por grupo de edad
INSERT INTO categories (name, display_name, age_group) VALUES
-- Niños (0)
('animales', 'Animales', 0),
('colores', 'Colores', 0),
('frutas', 'Frutas', 0),
('juguetes', 'Juguetes', 0),
('dibujos', 'Dibujos animados', 0),
('superheroes', 'Superhéroes', 0),
('dulces', 'Dulces', 0),
('deportes', 'Deportes', 0),
('instrumentos', 'Instrumentos', 0),
('planetas', 'Planetas', 0)
ON CONFLICT DO NOTHING;

INSERT INTO categories (name, display_name, age_group) VALUES
-- Adolescentes (1)
('musica', 'Música / Bandas', 1),
('peliculas', 'Películas', 1),
('series', 'Series TV', 1),
('videojuegos', 'Videojuegos', 1),
('tecnologia', 'Tecnología', 1),
('redes_sociales', 'Redes Sociales', 1),
('ropa', 'Ropa / Marcas', 1),
('lugares', 'Lugares / Ciudades', 1),
('famosos', 'Famosos', 1),
('libros', 'Libros', 1)
ON CONFLICT DO NOTHING;

INSERT INTO categories (name, display_name, age_group) VALUES
-- Adultos (2)
('nombres', 'Nombres', 2),
('paises', 'Países', 2),
('profesiones', 'Profesiones', 2),
('marcas', 'Marcas', 2),
('historia', 'Historia', 2),
('ciencia', 'Ciencia', 2),
('arte', 'Arte', 2),
('comida', 'Comida / Bebida', 2),
('automoviles', 'Automóviles', 2),
('literatura', 'Literatura', 2)
ON CONFLICT DO NOTHING;

-- Seed de logros
INSERT INTO achievements (name, description, icon, points, requirement_type, requirement_value) VALUES
-- Juegos jugados
('Primer STOP', 'Juega tu primera partida', '🎮', 10, 'games_played', 1),
('Calentando', 'Juega 10 partidas', '🔥', 50, 'games_played', 10),
('Veterano', 'Juega 50 partidas', '🏅', 100, 'games_played', 50),
-- Victorias
('Primera Victoria', 'Gana tu primera partida', '🏆', 20, 'games_won', 1),
('Triple Corona', 'Gana 10 partidas', '👑', 75, 'games_won', 10),
('Campeón', 'Gana 50 partidas', '⭐', 150, 'games_won', 50),
-- Score
('Puntuador', 'Alcanza 1000 puntos totales', '💯', 30, 'total_score', 1000),
('Estrella', 'Alcanza 10000 puntos totales', '🌟', 100, 'total_score', 10000),
('Leyenda', 'Alcanza 50000 puntos totales', '💎', 250, 'total_score', 50000),
-- Rachas
('Racha Inicial', 'Obtén una racha de 3 victorias', '⚡', 25, 'streak', 3),
('Racha Imparable', 'Obtén una racha de 10 victorias', '🚀', 100, 'streak', 10)
ON CONFLICT DO NOTHING;
