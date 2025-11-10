-- 11. 아이템 테이블 생성

CREATE TABLE items(
	item_id INT AUTO_INCREMENT PRIMARY KEY,
	`name` VARCHAR(100) NOT NULL,
	DESCRIPTION TEXT,
	VALUE INT DEFAULT 0
)

-- 12. 아이템 데이터 삽입
INSERT INTO items(name, DESCRIPTION, VALUE) VALUES
('검', '기본 무기', 10),
('방패', '기본 방어구', 15),
('물약', '체력을 회복', 5)

SELECT * FROM items

SELECT * FROM player_quests

-- 13. 플레이어 인벤토리 테이블 생성
CREATE TABLE inventorties(
	inventory_id INT AUTO_INCREMENT PRIMARY KEY,
	player_id INT,
	item_id INT,
	quantity INT DEFAULT 1,
	FOREIGN KEY(player_id) REFERENCES players(player_id),
	FOREIGN KEY(item_id) REFERENCES items(item_id)
)

-- 14. 인벤토리에 아이템 추가
INSERT INTO inventorties (player_id, item_id, quantity) VALUES
(1, 1, 1),
(1, 3, 5),
(2, 2, 1)

-- 15. 플레이어의 인벤토리 조회
SELECT p.username, i.name, inv.quantity
FROM players p
JOIN inventorties inv ON p.player_id = inv.player_id
JOIN items i ON inv.item_id = i.item_id

-- 실습
-- 1. 새로운 아이템 추가
INSERT INTO items(name, DESCRIPTION, VALUE) VALUES
('갑옷', '기본적인 방어구', 20)
-- 2. 특정 플레이어의 인벤토리에새 아이템 추가
INSERT INTO inventorties (player_id, item_id, quantity) VALUES
(4, 4, 1)
-- 3. 가장 가치 있는 아이템 찾기 ( ORDER BY value DESC)
SELECT name, VALUE FROM items ORDER BY VALUE DESC LIMIT 1

-- 17. 퀘스트 테이블 생성
CREATE TABLE quests(
	quest_id INT AUTO_INCREMENT PRIMARY KEY,
	title VARCHAR(100) NOT NULL,
	DESCRIPTION TEXT,
	reward_exp INT DEFAULT 0,
	reward_item_id INT,
	FOREIGN KEY (reward_item_id) REFERENCES items(item_id)
)

-- 18. 퀘스트 데이터 삽입
INSERT INTO quests(title, DESCRIPTION, reward_exp, reward_item_id) VALUES
('초보자 퀘스트', '첫번째 퀘스트를 완료하세요', 100, 3),
('용사의 검', '용사의 검을 찾아보세요', 500, 1)

-- 19. 플레이어 퀘스트 진행 상황 테이블
CREATE TABLE player_quests(
	player_id INT,
	quest_id INT,
	STATUS ENUM('시작', '진행중', '완료') DEFAULT '시작',
	start_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
	completed_at TIMESTAMP NULL,
	PRIMARY KEY (player_id, quest_id),
	FOREIGN KEY (player_id) REFERENCES players(player_id),
	FOREIGN KEY (quest_id) REFERENCES quests(quest_id)
)

-- 20. 플레이어에게 퀘스트 할당
INSERT INTO player_quests(player_id, quest_id) VALUES
(1, 1),
(2, 2)

-- 21. 진행중인 퀘스트 조회
SELECT p.username, q.title, pq.status
FROM players p
JOIN player_quests pq ON p.player_id = pq.player_id
JOIN quests q ON pq.quest_id = q.quest_id
WHERE pq.status != '완료'

-- 22. 퀘스트 완료 처리
UPDATE player_quests
SET STATUS ='완료', completed_at = CURRENT_TIMESTAMP
WHERE player_id = 1 AND quest_id = 1;

-- 실습
-- 1. 새로운 퀘스트 추가
INSERT INTO quests(title, DESCRIPTION, reward_exp, reward_item_id) VALUES
('보스처치 퀘스트', '보스를 처치하세요', 700, 4)
-- 2. 특정 플레이어의 모든 퀘스트 상태 조회
SELECT p.username, q.title, pq.status
FROM players p
JOIN player_quests pq ON p.player_id = pq.player_id
JOIN quests q ON pq.quest_id = q.quest_id
WHERE p.player_id = 1
-- 3. 가장 많은경험치를 주는 퀘스트 출력
SELECT title, reward_exp FROM quests ORDER BY reward_exp DESC LIMIT 1

SELECT * FROM player_quests