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