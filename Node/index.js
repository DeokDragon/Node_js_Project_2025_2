const express = require('express');
const fs = require('fs');
const playerRoutes = require('./Routes/playerRoutes');
const app = express();
const port = 4000;

app.use(express.json());
app.use('/api', playerRoutes);
const resourceFilePath = 'resources.json';


function loadResource()
{
    if(fs.existsSync(resourceFilePath))
    {
        const data = fs.readFileSync(resourceFilePath);
        global.players = JSON.parse(data);
    }
    else
    {
        global.players = {};
    }
}

function saveResources()
{
    fs.writeFileSync(resourceFilePath, JSON.stringify(global.player, null, 2));
}

app.listen(port, ()=>
{
    console.log('서버가 http://localhost:${port}에서 실행 중 입니다.');
})