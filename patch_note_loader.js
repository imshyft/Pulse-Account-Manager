async function main() {
    const url = `https://overwatch.blizzard.com/en-us/news/patch-notes/`;
    let request = new XMLHttpRequest();
    request.open("GET", url, true);
    new Promise(function(resolve, reject) {
        request.onreadystatechange = function() {
            if (this.readyState == 4 && this.status == 200) {
                const x = document.createElement("div")
                x.innerHTML = request.responseText;
                document.getElementById("patch").innerHTML = x.getElementsByClassName("PatchNotes-body").item(0).innerHTML;
                x.remove();
            } else {
                resolve.onerror = reject;
            }
        }
        request.send();
    });
}

main();