/* scene-skyline.js -- NYC skyline scan hallucination
   Three.js city skyline with target buildings, rising particles,
   and a slow horizontal camera pan. Accent-lit windows dot the
   landmark towers. Loops with static flash on pan reset. */

import * as THREE from 'three';

(function () {
    'use strict';

    // -- bail if WebGL unavailable --
    const testCanvas = document.createElement('canvas');
    const gl = testCanvas.getContext('webgl') || testCanvas.getContext('experimental-webgl');
    if (!gl) return;

    const container = document.getElementById('nyc-skyline');
    if (!container) return;

    // -- reduced motion preference --
    const prefersReducedMotion = window.matchMedia('(prefers-reduced-motion: reduce)').matches;
    const speedMultiplier = prefersReducedMotion ? 0 : 1.0;

    // -- colors --
    const COLORS = {
        bg: 0x0A0E14,
        bgSurface: 0x12171F,
        accent: 0xE8A031,
        accentDim: 0x8B6320,
        teal: 0x5CCFE6,
    };

    // -- create canvas --
    const canvas = document.createElement('canvas');
    canvas.style.pointerEvents = 'none';
    canvas.setAttribute('aria-hidden', 'true');
    container.appendChild(canvas);

    // -- static flash overlay (for loop reset) --
    const flashOverlay = document.createElement('div');
    flashOverlay.style.cssText = [
        'position: absolute',
        'inset: 0',
        'background: white',
        'pointer-events: none',
        'z-index: 4',
        'opacity: 0',
        'transition: opacity 0.05s',
    ].join(';');
    container.appendChild(flashOverlay);

    // -- renderer --
    const renderer = new THREE.WebGLRenderer({ canvas, antialias: false, alpha: false });
    renderer.setPixelRatio(Math.min(window.devicePixelRatio, 2));
    renderer.setClearColor(COLORS.bg, 1);

    // -- scene --
    const scene = new THREE.Scene();

    // -- camera --
    let aspect = container.clientWidth / container.clientHeight;
    const camera = new THREE.PerspectiveCamera(45, aspect, 0.1, 100);
    camera.position.set(-15, 2, 20);

    // -- target buildings definition --
    const TARGET_BUILDINGS = [
        { name: '30_ROCK', x: -12, height: 14, width: 2.5 },
        { name: 'METLIFE', x: -4, height: 12, width: 3 },
        { name: 'WOOLWORTH', x: 5, height: 16, width: 2 },
        { name: 'ONE_WTC', x: 14, height: 22, width: 1.8 },
    ];

    // -- materials --
    const bgBuildingMat = new THREE.MeshBasicMaterial({ color: COLORS.bgSurface });
    const targetBuildingMat = new THREE.MeshBasicMaterial({ color: COLORS.accentDim });
    const groundMat = new THREE.MeshBasicMaterial({ color: COLORS.bg });

    // -- background buildings (~40) --
    for (let i = 0; i < 40; i++) {
        const w = 1 + Math.random() * 2;
        const h = 3 + Math.random() * 7;
        const x = -30 + Math.random() * 60;
        const geo = new THREE.BoxGeometry(w, h, 1);
        const mesh = new THREE.Mesh(geo, bgBuildingMat);
        mesh.position.set(x, h / 2, 0);
        scene.add(mesh);
    }

    // -- target buildings --
    const targetMeshes = [];
    for (let i = 0; i < TARGET_BUILDINGS.length; i++) {
        const b = TARGET_BUILDINGS[i];
        const geo = new THREE.BoxGeometry(b.width, b.height, 1);
        const mesh = new THREE.Mesh(geo, targetBuildingMat);
        mesh.position.set(b.x, b.height / 2, 0.5);
        scene.add(mesh);
        targetMeshes.push(mesh);

        // -- window dots on front face --
        const windowMat = new THREE.MeshBasicMaterial({
            color: COLORS.accent,
            transparent: true,
        });
        const windowGeo = new THREE.PlaneGeometry(0.15, 0.15);
        const cols = Math.floor(b.width / 0.4);
        const rows = Math.floor(b.height / 0.5);
        const startX = b.x - (cols - 1) * 0.4 / 2;
        const startY = 0.5;

        for (let row = 0; row < rows; row++) {
            for (let col = 0; col < cols; col++) {
                if (Math.random() > 0.6) continue; // 60% coverage
                const wMat = windowMat.clone();
                wMat.opacity = 0.3 + Math.random() * 0.5;
                const win = new THREE.Mesh(windowGeo, wMat);
                win.position.set(
                    startX + col * 0.4,
                    startY + row * 0.5,
                    1.01
                );
                scene.add(win);
            }
        }
    }

    // -- ground plane --
    const groundGeo = new THREE.BoxGeometry(70, 0.2, 4);
    const ground = new THREE.Mesh(groundGeo, groundMat);
    ground.position.set(0, -0.1, 0);
    scene.add(ground);

    // -- particles (60, rising from target buildings) --
    const PARTICLE_COUNT = 60;
    const particleGeo = new THREE.BufferGeometry();
    const particlePositions = new Float32Array(PARTICLE_COUNT * 3);
    const particleSpeeds = new Float32Array(PARTICLE_COUNT);
    const particleBuildingIndex = new Uint8Array(PARTICLE_COUNT);

    function initParticle(i) {
        const bIdx = Math.floor(Math.random() * TARGET_BUILDINGS.length);
        const b = TARGET_BUILDINGS[bIdx];
        const idx = i * 3;
        particlePositions[idx] = b.x + (Math.random() - 0.5) * b.width;
        particlePositions[idx + 1] = b.height * 0.3 + Math.random() * b.height * 0.7;
        particlePositions[idx + 2] = 0.5 + Math.random() * 0.5;
        particleSpeeds[i] = 0.5 + Math.random();
        particleBuildingIndex[i] = bIdx;
    }

    for (let i = 0; i < PARTICLE_COUNT; i++) {
        initParticle(i);
    }

    particleGeo.setAttribute('position', new THREE.BufferAttribute(particlePositions, 3));

    // circular particle texture
    const particleCanvas = document.createElement('canvas');
    particleCanvas.width = 32;
    particleCanvas.height = 32;
    const pCtx = particleCanvas.getContext('2d');
    pCtx.beginPath();
    pCtx.arc(16, 16, 14, 0, Math.PI * 2);
    pCtx.fillStyle = '#ffffff';
    pCtx.fill();
    const particleTexture = new THREE.CanvasTexture(particleCanvas);

    const particleMat = new THREE.PointsMaterial({
        color: COLORS.accent,
        size: 0.12,
        map: particleTexture,
        transparent: true,
        opacity: 0.7,
        depthWrite: false,
        blending: THREE.AdditiveBlending,
        sizeAttenuation: true,
    });

    const particles = new THREE.Points(particleGeo, particleMat);
    scene.add(particles);

    // -- scene label cycling --
    const LABEL_TEXTS = [
        'S.L.O.P.://NYC_SECTOR [SCANNING]',
        'S.L.O.P.://30_ROCK [THREAT: LOW]',
        'S.L.O.P.://METLIFE [THREAT: MODERATE]',
        'S.L.O.P.://WOOLWORTH [THREAT: HIGH]',
        'S.L.O.P.://ONE_WTC [THREAT: EXTREME]',
    ];
    const sceneLabel = container.querySelector('.scene-label') ||
        container.parentElement.querySelector('.scene-label');
    let labelIndex = 0;
    if (sceneLabel) {
        sceneLabel.textContent = LABEL_TEXTS[0];
        setInterval(function () {
            labelIndex = (labelIndex + 1) % LABEL_TEXTS.length;
            sceneLabel.textContent = LABEL_TEXTS[labelIndex];
        }, 3000);
    }

    // -- timing --
    const PAN_DURATION = 16.0;
    const CAMERA_START_X = -15;
    const CAMERA_END_X = 18;

    // -- state --
    let loopTime = 0;
    let flashActive = false;
    const clock = new THREE.Clock();

    // -- sizing --
    function updateSize() {
        const w = container.clientWidth;
        const h = container.clientHeight;
        aspect = w / h;
        camera.aspect = aspect;
        camera.updateProjectionMatrix();
        renderer.setSize(w, h);
    }
    updateSize();

    // -- resize handler --
    window.addEventListener('resize', updateSize);

    // -- static flash for loop reset --
    function triggerFlash() {
        if (flashActive) return;
        flashActive = true;
        flashOverlay.style.opacity = '0.8';
        setTimeout(function () {
            flashOverlay.style.opacity = '0';
            flashActive = false;
        }, 100);
    }

    // -- animation loop --
    function animate() {
        requestAnimationFrame(animate);

        const delta = Math.min(clock.getDelta(), 0.1) * speedMultiplier;

        // advance loop time
        loopTime += delta;

        // -- camera pan --
        const panProgress = Math.min(loopTime / PAN_DURATION, 1.0);
        const cameraX = CAMERA_START_X + (CAMERA_END_X - CAMERA_START_X) * panProgress;
        camera.position.set(cameraX, 2, 20);
        camera.lookAt(cameraX + 2, 3, 0);

        // -- particles: rise and reset --
        const posAttr = particleGeo.getAttribute('position');
        for (let i = 0; i < PARTICLE_COUNT; i++) {
            const idx = i * 3;
            posAttr.array[idx + 1] += particleSpeeds[i] * delta;

            // reset when above building top + 3
            const b = TARGET_BUILDINGS[particleBuildingIndex[i]];
            if (posAttr.array[idx + 1] > b.height + 3) {
                posAttr.array[idx] = b.x + (Math.random() - 0.5) * b.width;
                posAttr.array[idx + 1] = b.height * 0.3;
                posAttr.array[idx + 2] = 0.5 + Math.random() * 0.5;
                particleSpeeds[i] = 0.5 + Math.random();
            }
        }
        posAttr.needsUpdate = true;

        // -- loop reset --
        if (panProgress >= 1.0) {
            triggerFlash();
            loopTime = 0;
        }

        renderer.render(scene, camera);
    }

    animate();
})();
