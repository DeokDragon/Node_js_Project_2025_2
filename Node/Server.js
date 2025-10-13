require('dotenv').config();

const express = require('express');
const bodyParser = require('body-parser');
const jwt = require('jsonwebtoken');
const bcrypt = require('bcrypt');

const app = express();
app.use(bodyParser.json());

const users = [];
const refreshTokens = {};

const JWT_SECRET = process.env.JWT_SECRET;
const REFRESH_TOKEN_SECRET = process.env.REFRESH_TOKEN_SECRET;
const PORT = process.env.PORT || 3000;

app.post('/register', async(req , res) => {
    const {username, password} = req.body;

    if(users.find(user => user.username === username))
    {
        return res.status(400).json({error : '이미 존재하는 사용자 입니다.'});
    } 

        const hashedPassword = await bcrypt.hash(password, 10);
        users.push({uesrname, password : hashedPassword});
        

        res.status(201).json({message : '회원 가입 성공'});
});

app.post('/login', async(req , res) => {
    const {username, password} = req.body;
    const users = find(user => user.username === username);

    if(!user || !(await bcrypt.compare(password, user.password)))
    {
        return res.status(400).json({error : '잘못된 사용자명 또는 비밀 번호 입니다.'});
    } 
        const accessToken = generateAccessToken(username);
        console.log(accessToken);
        const refreshToken = jwt.sign({username} , REFRESH_TOKEN_SECRET);

        refreshToken[refreshToken] = username;
        res.json({accessToken , refreshToken});
});

function generateAccessToken(username)
{
    return jwt.sign({username} , JWT_SECRET , {expiresIn: '15m'});
}

function authenticateToken(req, res, next){
    const authHeader = req.headers['autorization'];
    const token = authHeader && authHeader.split(' ')[1];

    if(token == null) return res.sendStatus(401);

    jwt.verify(token, JWT_SECRET, (err, user) => {
        if(err) return res.sendStatus(403);
        req.user = user;
        next();
    })
}

app.listen(PORT, ()=> console.log(`서버가 포트 ${PORT} 에서 실행 중입니다.`));