const { app, BrowserWindow } = require('electron');
const { XMLHttpRequest } = require("xmlhttprequest");
const fs = require("fs");

const createWindow = () => {
    const win = new BrowserWindow({
        width: 800,
        height: 600
    })

    win.loadFile('index.html');
}

app.whenReady().then(() => {
    createWindow();

    player_summary("PlateOfSuki", "3588").then((json) => {
        console.log(json);
        player_file("PlateOfSuki", "3588", json);
    })
})

app.on('window-all-closed', () => {
    if (process.platform !== 'darwin') app.quit();

    app.on('activate', () => {
        if (BrowserWindow.getAllWindows().length === 0) createWindow();
    })
})


// TEMPORARY HERE FOR TESTING
async function player_file(username, tag, json) {
    fs.readFile(`test_data/${username}-${tag}.json`, 'utf8', function readFileCallback(err, data) {
        let obj = {};
        if (err) {
            console.log(err);
            if (err.errno === -4058) {
                obj = {
                    "username": username,
                    "tag": tag,
                    "email": "__email__",
                    "avatar": "__avatar_png__",
                    "last_update": "0",
                    "times_switched": "0",
                    "times_launched": "0",
                    "rank_histories": {
                        "tank": {},
                        "damage": {},
                        "support": {},
                        "highest": {
                            "tank": {"rating": "0", "date": "0"},
                            "damage": {"rating": "0", "date": "0"},
                            "support": {"rating": "0", "date": "0"}
                        }
                    }
                }
            } else {
                return;
            }
        } else {
            obj = JSON.parse(data);
        }
        obj.last_update = json.last_updated_at;
        const comp = json.competitive.pc;
        obj.avatar = json.avatar;
        if (comp.tank != null) {
            const sr = div_to_sr(comp.tank.division, comp.tank.tier);
            obj.rank_histories.tank[json.last_updated_at] = sr;
            if (sr >= obj.rank_histories.highest.tank.rating) {
                obj.rank_histories.highest.tank.rating = sr;
                obj.rank_histories.highest.tank.date = json.last_updated_at;
            }
        }
        if (comp.damage != null) {
            const sr = div_to_sr(comp.damage.division, comp.damage.tier);
            obj.rank_histories.damage[json.last_updated_at] = sr;
            if (sr >= obj.rank_histories.highest.damage.rating) {
                obj.rank_histories.highest.damage.rating = sr;
                obj.rank_histories.highest.damage.date = json.last_updated_at;
            }
        }
        if (comp.support != null) {
            const sr = div_to_sr(comp.support.division, comp.support.tier);
            obj.rank_histories.support[json.last_updated_at] = sr;
            if (sr >= obj.rank_histories.highest.support.rating) {
                obj.rank_histories.highest.support.rating = sr;
                obj.rank_histories.highest.support.date = json.last_updated_at;
            }
        }
        fs.writeFile(`test_data/${username}-${tag}.json`, JSON.stringify(obj), 'utf8', function() {console.log("done")});
    });
}

const divisions = {"bronze": 1000, "silver": 1500, "gold": 2000, "platinum": 2500, "diamond": 3000, "master": 3500, "grandmaster": 4000, "champion" : 4500};

async function player_summary(username, tag) {
    const url = `https://overfast-api.tekrop.fr/players/${username}-${tag}/summary`;
    let request = new XMLHttpRequest();
    request.open("GET", url, true);
    return new Promise(function(resolve, reject) {
        request.onreadystatechange = function() {
            if (this.readyState == 4 && this.status == 200) {
                let json = JSON.parse(request.responseText);
                resolve(json);
            } else {
                resolve.onerror = reject;
            }
        }
        request.send();
    });
}

function div_to_sr(div, tier) {
    return divisions[div] + (500 - tier * 100);
}
