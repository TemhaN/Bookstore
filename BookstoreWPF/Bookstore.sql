-- Таблица пользователей
CREATE TABLE users (
    user_id INT PRIMARY KEY IDENTITY(1,1),
    username VARCHAR(50) NOT NULL UNIQUE,
    password VARCHAR(255) NOT NULL,
    email VARCHAR(100) NOT NULL UNIQUE,
    created_at DATETIME DEFAULT GETDATE()
);

-- Таблица школ (если нужна привязка к школам)
CREATE TABLE schools (
    school_id INT PRIMARY KEY IDENTITY(1,1),
    school_name VARCHAR(100) NOT NULL,
    address VARCHAR(255),
    contact_info VARCHAR(100)
);

-- Таблица категорий книг
CREATE TABLE categories (
    category_id INT PRIMARY KEY IDENTITY(1,1),
    category_name VARCHAR(50) NOT NULL,
    description TEXT
);

-- Таблица книг
CREATE TABLE books (
    book_id INT PRIMARY KEY IDENTITY(1,1),
    title VARCHAR(200) NOT NULL,
    author VARCHAR(100) NOT NULL,
    price DECIMAL(10,2) NOT NULL,
    description TEXT,
    category_id INT,
    stock_quantity INT NOT NULL DEFAULT 0,
    short_info VARCHAR(255),
    FOREIGN KEY (category_id) REFERENCES categories(category_id)
);

-- Таблица корзины
CREATE TABLE cart (
    cart_id INT PRIMARY KEY IDENTITY(1,1),
    user_id INT,
    created_at DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (user_id) REFERENCES users(user_id)
);

-- Таблица элементов корзины
CREATE TABLE cart_items (
    cart_item_id INT PRIMARY KEY IDENTITY(1,1),
    cart_id INT,
    book_id INT,
    quantity INT NOT NULL DEFAULT 1,
    FOREIGN KEY (cart_id) REFERENCES cart(cart_id),
    FOREIGN KEY (book_id) REFERENCES books(book_id)
);

-- Таблица заказов
CREATE TABLE orders (
    order_id INT PRIMARY KEY IDENTITY(1,1),
    user_id INT,
    total_amount DECIMAL(10,2) NOT NULL,
    order_status VARCHAR(20) CHECK (order_status IN ('pending', 'processed', 'shipped', 'completed', 'cancelled')) DEFAULT 'pending',
    created_at DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (user_id) REFERENCES users(user_id)
);

-- Таблица элементов заказа
CREATE TABLE order_items (
    order_item_id INT PRIMARY KEY IDENTITY(1,1),
    order_id INT,
    book_id INT,
    quantity INT NOT NULL,
    price_at_time DECIMAL(10,2) NOT NULL,
    FOREIGN KEY (order_id) REFERENCES orders(order_id),
    FOREIGN KEY (book_id) REFERENCES books(book_id)
);

ALTER TABLE books ALTER COLUMN title NVARCHAR(200) NOT NULL;
ALTER TABLE books ALTER COLUMN author NVARCHAR(100) NOT NULL;
ALTER TABLE books ALTER COLUMN description NVARCHAR(MAX);
ALTER TABLE books ALTER COLUMN short_info NVARCHAR(255);



-- Заполнение таблицы users
INSERT INTO users (username, password, email) VALUES
('john_doe', 'hashed_pass1', 'john@example.com'),
('mary_smith', 'hashed_pass2', 'mary@example.com'),
('peter_parker', 'hashed_pass3', 'peter@example.com'),
('anna_wilson', 'hashed_pass4', 'anna@example.com'),
('bob_jones', 'hashed_pass5', 'bob@example.com'),
('clara_brown', 'hashed_pass6', 'clara@example.com'),
('david_lee', 'hashed_pass7', 'david@example.com'),
('emma_white', 'hashed_pass8', 'emma@example.com'),
('frank_miller', 'hashed_pass9', 'frank@example.com'),
('grace_kelly', 'hashed_pass10', 'grace@example.com');

-- Заполнение таблицы schools
INSERT INTO schools (school_name, address, contact_info) VALUES
(N'Школа №1', N'ул. Ленина 10', '+7-999-111-2233'),
(N'Школа №2', N'ул. Мира 5', '+7-999-222-3344'),
(N'Школа №3', N'ул. Победы 15', '+7-999-333-4455'),
(N'Школа №4', N'пр. Гагарина 22', '+7-999-444-5566'),
(N'Школа №5', N'ул. Советская 8', '+7-999-555-6677'),
(N'Школа №6', N'ул. Южная 17', '+7-999-666-7788'),
(N'Школа №7', N'ул. Северная 3', '+7-999-777-8899'),
(N'Школа №8', N'ул. Центральная 12', '+7-999-888-9900');

-- Заполнение таблицы categories
INSERT INTO categories (category_name, description) VALUES
(N'Фантастика', N'Книги о вымышленных мирах и будущем'),
(N'Классика', N'Произведения классической литературы'),
(N'Детектив', N'Детективные истории и расследования'),
(N'Фэнтези', N'Мир магии и приключений'),
(N'Научная литература', N'Книги по науке и исследованиям'),
(N'Детская литература', N'Книги для детей и подростков');

-- Заполнение таблицы books
INSERT INTO books (title, author, price, description, category_id, stock_quantity, short_info) VALUES
(N'Дюна', N'Фрэнк Герберт', 599.99, N'Эпическая сага о пустынной планете', 1, 15, N'Культовый sci-fi роман'),
(N'1984', N'Джордж Оруэлл', 399.00, N'Антиутопия о тоталитаризме', 1, 20, N'Классика антиутопии'),
(N'Гордость и предубеждение', N'Джейн Остин', 349.50, N'История любви и предрассудков', 2, 25, N'Классика романтики'),
(N'Преступление и наказание', N'Фёдор Достоевский', 450.00, N'Психологический роман', 2, 18, N'Русская классика'),
(N'Собака Баскервилей', N'Артур Конан Дойл', 429.00, N'Загадка на болотах', 3, 12, N'Шерлок Холмс в деле'),
(N'Убийство в Восточном экспрессе', N'Агата Кристи', 389.90, N'Детектив на поезде', 3, 15, N'Эркюль Пуаро расследует'),
(N'Властелин колец', N'Дж. Р. Р. Толкин', 799.00, N'Эпическое фэнтези', 4, 10, N'Легенда фэнтези'),
(N'Гарри Поттер и Философский камень', N'Дж. К. Роулинг', 550.00, N'Начало волшебной саги', 4, 30, N'Мир магии для всех'),
(N'Космос', N'Карл Саган', 650.00, N'Исследование вселенной', 5, 8, N'Наука о космосе'),
(N'Краткая история времени', N'Стивен Хокинг', 499.99, N'О времени и пространстве', 5, 12, N'Физика для всех'),
(N'Маленький принц', N'Антуан де Сент-Экзюпери', 299.00, N'Сказка о дружбе', 6, 25, N'Детская классика'),
(N'Винни-Пух', N'Алан Милн', 320.00, N'Приключения медвежонка', 6, 20, N'Любимая детская книга'),
(N'Хроники Нарнии', N'К. С. Льюис', 600.00, N'Фэнтези для детей', 6, 15, N'Волшебный мир Нарнии'),
(N'Тайна третьей планеты', N'Кир Булычёв', 350.00, N'Космические приключения Алисы', 1, 18, N'Советская фантастика'),
(N'Мастер и Маргарита', N'Михаил Булгаков', 470.00, N'Мистика и сатира', 2, 22, N'Шедевр литературы');

-- Заполнение таблицы cart
INSERT INTO cart (user_id) VALUES
(1), (2), (3), (5), (7), (9);

-- Заполнение таблицы cart_items
INSERT INTO cart_items (cart_id, book_id, quantity) VALUES
(1, 1, 2), (1, 5, 1), (2, 3, 1), (2, 8, 2), (3, 7, 1), (3, 9, 1),
(4, 2, 1), (4, 11, 3), (5, 4, 1), (5, 15, 1), (6, 6, 2), (6, 10, 1),
(1, 13, 1), (3, 14, 2), (5, 12, 1);

-- Заполнение таблицы orders
INSERT INTO orders (user_id, total_amount, order_status) VALUES
(1, 1628.98, 'completed'), (2, 1449.50, 'shipped'), (3, 2149.00, 'pending'),
(4, 1296.97, 'processed'), (6, 599.00, 'completed'), (7, 920.00, 'shipped'),
(8, 799.00, 'pending'), (10, 970.00, 'completed');

-- Заполнение таблицы order_items
INSERT INTO order_items (order_id, book_id, quantity, price_at_time) VALUES
(1, 1, 2, 599.99), (1, 5, 1, 429.00), (2, 3, 1, 349.50), (2, 8, 2, 550.00),
(3, 7, 1, 799.00), (3, 14, 2, 675.00), (4, 2, 1, 399.00), (4, 11, 3, 299.00),
(5, 11, 2, 299.00), (6, 4, 1, 450.00), (6, 15, 1, 470.00), (7, 7, 1, 799.00),
(8, 9, 1, 650.00), (8, 12, 1, 320.00), (3, 9, 1, 650.00), (1, 13, 1, 600.00),
(4, 6, 1, 389.90), (6, 10, 1, 499.99), (2, 12, 1, 320.00), (8, 5, 1, 429.00);