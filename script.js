const canvas = document.querySelector("#unity-canvas");

const gameVersionSelector = document.querySelector("#game-version-selector");
const playBtn = document.querySelector("#play-button");
const fullscreenBtn = document.querySelector("#fullscreen-button");
const earlyAccessText = document.querySelector("#ea-warning");

const baseUrl = "Builds";
let currentVersion = "";

function loadGameVersion(version) {
    currentVersion = version;
    playBtn.classList.remove("highlight");
    playBtn.classList.add("disabled");
    earlyAccessText.classList.toggle("hidden", version == "Latest");

    const buildUrl = `${baseUrl}/${version}/${version}`;
    const loaderUrl = buildUrl + ".loader.js";

    const config = {
        arguments: [],
        dataUrl: buildUrl + ".data.unityweb",
        frameworkUrl: buildUrl + ".framework.js.unityweb",
        codeUrl: buildUrl + ".wasm.unityweb",
        streamingAssetsUrl: "StreamingAssets",
        companyName: "Team 2",
        productName: "Tag-Team Takedown",
        productVersion: version,
    };

    let oldScript = document.querySelector("#unity-loader-script");
    if (oldScript) oldScript.remove();

    const unityLoaderScript = document.createElement("script");
    unityLoaderScript.id = "unity-loader-script";

    unityLoaderScript.onload = () => {
        createUnityInstance(canvas, config, (progress) => {})
            .then((unityInstance) => {
                fullscreenBtn.onclick = () => {
                    unityInstance.SetFullscreen(1);
                };

                playBtn.onclick = () => {
                    unityInstance.Quit();
                    loadSelectedGameVersion();
                };
            })
            .catch((message) => {
                alert(message);
            });
    };

    unityLoaderScript.src = loaderUrl;
    document.body.appendChild(unityLoaderScript);
}

function selectGameVersion(version) {
    playBtn.classList.toggle("disabled", version === currentVersion);
}

function loadSelectedGameVersion() {
    const selectedVersion = gameVersionSelector.value;
    if (selectedVersion !== currentVersion) {
        loadGameVersion(selectedVersion);
    }
}

function turnImageCarousel(id, direction) {
    const carousel = document.querySelector(`#${id}`);
    const image = carousel.querySelector("img");
    const pageIndicator = carousel.querySelector(".carousel-controls p");

    let imagePath = carousel.getAttribute("data-path");
    let currentIndex = parseInt(carousel.getAttribute("data-current")) || 1;
    const totalImages = parseInt(carousel.getAttribute("data-last")) || 1;

    currentIndex += direction;
    if (currentIndex < 1) {
        currentIndex = totalImages;
    } else if (currentIndex > totalImages) {
        currentIndex = 1;
    }

    image.src = `${imagePath}/${currentIndex}.png`;
    carousel.setAttribute("data-current", currentIndex);
    pageIndicator.textContent = `${currentIndex}/${totalImages}`;
}
