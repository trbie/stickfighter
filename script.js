const canvas = document.querySelector("#unity-canvas");

const gameVersionSelector = document.querySelector("#game-version-selector");
const loadingIndictor = document.querySelector("#unity-loading-indicator");
const playBtn = document.querySelector("#play-button");
const fullscreenBtn = document.querySelector("#fullscreen-button");
const earlyAccessText = document.querySelector("#ea-warning");

const baseUrl = "Builds";
let currentVersion = "";
let currentUnityInstance = null;
let isLoading = false;

loadGameVersion("Latest");

function cleanupCanvas() {
    // Clear WebGL context
    const gl = canvas.getContext("webgl") || canvas.getContext("experimental-webgl");
    if (gl) {
        const extension = gl.getExtension("WEBGL_lose_context");
        if (extension) {
            extension.loseContext();
        }
    }

    // Reset canvas completely
    const parent = canvas.parentNode;
    const newCanvas = canvas.cloneNode(true);
    parent.replaceChild(newCanvas, canvas);

    // Update global reference
    const canvasVar = document.querySelector("#unity-canvas");
    return canvasVar;
}

function loadGameVersion(version) {
    if (isLoading) {
        console.warn("Already loading a game version");
        return;
    }

    isLoading = true;
    currentVersion = version;
    playBtn.classList.remove("highlight");
    playBtn.classList.add("disabled");
    earlyAccessText.classList.toggle("hidden", version == "Latest");

    // Clean up previous instance
    if (currentUnityInstance) {
        try {
            currentUnityInstance.Quit();
        } catch (e) {
            console.warn("Failed to quit previous Unity instance:", e);
        }
        currentUnityInstance = null;
    }

    // Clean and recreate canvas
    const newCanvas = cleanupCanvas();
    newCanvas.width = 960;
    newCanvas.height = 600;
    newCanvas.style.width = "960px";
    newCanvas.style.height = "600px";

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
        matchWebGLToCanvasSize: false,
        devicePixelRatio: 1,
        webglContextAttributes: {
            preserveDrawingBuffer: false,
            powerPreference: "default",
        },
    };
    let oldScript = document.querySelector("#unity-loader-script");
    if (oldScript) oldScript.remove();

    const unityLoaderScript = document.createElement("script");
    unityLoaderScript.id = "unity-loader-script";

    unityLoaderScript.onload = () => {
        loadingIndictor.classList.remove("hidden");
        loadingIndictor.classList.remove("error");
        createUnityInstance(newCanvas, config, (progress) => {
            loadingIndictor.textContent = `Loading... (${Math.floor(progress * 100)}%)`;
        })
            .then((unityInstance) => {
                currentUnityInstance = unityInstance;
                loadingIndictor.classList.add("hidden");
                isLoading = false;

                playBtn.classList.remove("disabled");
                playBtn.classList.add("highlight");

                fullscreenBtn.onclick = () => {
                    unityInstance.SetFullscreen(1);
                };

                playBtn.onclick = () => {
                    unityInstance.Quit();
                    currentUnityInstance = null;
                    loadSelectedGameVersion();
                };
            })
            .catch((message) => {
                console.error("Unity loading failed:", message);
                loadingIndictor.textContent = "Failed to load game";
                loadingIndictor.classList.add("error");
                loadingIndictor.classList.remove("hidden");
                playBtn.classList.remove("disabled");
                playBtn.classList.add("highlight");
                isLoading = false;
            });
    };

    unityLoaderScript.onerror = () => {
        console.error("Failed to load Unity loader script");
        loadingIndictor.textContent = "Failed to load game files";
        loadingIndictor.classList.add("error");
        loadingIndictor.classList.remove("hidden");
        playBtn.classList.remove("disabled");
        playBtn.classList.add("highlight");
        isLoading = false;
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
