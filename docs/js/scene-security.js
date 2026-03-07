/* scene-security.js -- SLOP sector scan hallucination
   Three.js first-person security camera feed for the explore page.
   Renders a corridor with bioluminescent particles, ceiling pipes,
   a creature shadow, and a missing wall section. Green night-vision
   tint via DOM overlay. Loops with static flash on reset. */

import * as THREE from 'three';

(function () {
    'use strict';

    // -- bail if WebGL unavailable --
    const testCanvas = document.createElement('canvas');
    const gl = testCanvas.getContext('webgl') || testCanvas.getContext('experimental-webgl');
    if (!gl) return;

    const container = document.getElementById('sector-scan');
    if (!container) return;

    // -- reduced motion preference --
    const prefersReducedMotion = window.matchMedia('(prefers-reduced-motion: reduce)').matches;
    const speedMultiplier = prefersReducedMotion ? 0.4 : 1.0;

    // -- colors --
    const COLORS = {
        bg: 0x0A0E14,
        wall: 0x1A1F2A,
        ceiling: 0x12171F,
        floor: 0x1A1F2A,
        pipe: 0x2A2F3A,
        particle: 0x5CCFE6,
        creature: 0x050505,
    };

    // -- create canvas --
    const canvas = document.createElement('canvas');
    canvas.style.pointerEvents = 'none';
    canvas.setAttribute('aria-hidden', 'true');
    container.appendChild(canvas);

    // -- green tint overlay --
    const greenOverlay = document.createElement('div');
    greenOverlay.style.cssText = [
        'background: rgba(0, 255, 65, 0.12)',
        'mix-blend-mode: multiply',
        'pointer-events: none',
        'position: absolute',
        'inset: 0',
        'z-index: 2',
    ].join(';');
    container.appendChild(greenOverlay);

    // -- grain overlay --
    const grainCanvas = document.createElement('canvas');
    grainCanvas.width = 128;
    grainCanvas.height = 128;
    grainCanvas.style.cssText = [
        'position: absolute',
        'inset: 0',
        'width: 100%',
        'height: 100%',
        'pointer-events: none',
        'z-index: 3',
        'opacity: 0.08',
        'mix-blend-mode: screen',
    ].join(';');
    container.appendChild(grainCanvas);
    const grainCtx = grainCanvas.getContext('2d');

    let grainIntensity = 0.08;
    function updateGrain() {
        const w = grainCanvas.width;
        const h = grainCanvas.height;
        const imageData = grainCtx.createImageData(w, h);
        const data = imageData.data;
        for (let i = 0; i < data.length; i += 4) {
            const v = Math.random() * 255;
            data[i] = v;
            data[i + 1] = v;
            data[i + 2] = v;
            data[i + 3] = 255;
        }
        grainCtx.putImageData(imageData, 0, 0);
        grainCanvas.style.opacity = String(grainIntensity);
    }

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

    // -- first-person perspective camera --
    let aspect = container.clientWidth / container.clientHeight;
    const camera = new THREE.PerspectiveCamera(65, aspect, 0.1, 50);

    // -- corridor dimensions --
    const CORRIDOR_LENGTH = 18;
    const CORRIDOR_WIDTH = 3;
    const CORRIDOR_HEIGHT = 3;
    const HALF_W = CORRIDOR_WIDTH / 2;
    const HALF_H = CORRIDOR_HEIGHT / 2;

    // -- build corridor geometry --
    const wallMat = new THREE.MeshBasicMaterial({ color: COLORS.wall });
    const ceilingMat = new THREE.MeshBasicMaterial({ color: COLORS.ceiling });
    const floorMat = new THREE.MeshBasicMaterial({ color: COLORS.floor });
    const pipeMat = new THREE.MeshBasicMaterial({ color: COLORS.pipe });

    // floor
    const floorGeo = new THREE.BoxGeometry(CORRIDOR_WIDTH, 0.1, CORRIDOR_LENGTH);
    const floor = new THREE.Mesh(floorGeo, floorMat);
    floor.position.set(0, -HALF_H, CORRIDOR_LENGTH / 2);
    scene.add(floor);

    // ceiling
    const ceilingGeo = new THREE.BoxGeometry(CORRIDOR_WIDTH, 0.1, CORRIDOR_LENGTH);
    const ceiling = new THREE.Mesh(ceilingGeo, ceilingMat);
    ceiling.position.set(0, HALF_H, CORRIDOR_LENGTH / 2);
    scene.add(ceiling);

    // left wall (full length)
    const leftWallGeo = new THREE.BoxGeometry(0.1, CORRIDOR_HEIGHT, CORRIDOR_LENGTH);
    const leftWall = new THREE.Mesh(leftWallGeo, wallMat);
    leftWall.position.set(-HALF_W, 0, CORRIDOR_LENGTH / 2);
    scene.add(leftWall);

    // right wall -- with a gap (missing section) between z=8 and z=11
    // front section: z=0 to z=8
    const rwFrontGeo = new THREE.BoxGeometry(0.1, CORRIDOR_HEIGHT, 8);
    const rwFront = new THREE.Mesh(rwFrontGeo, wallMat);
    rwFront.position.set(HALF_W, 0, 4); // centered at z=4
    scene.add(rwFront);

    // back section: z=11 to z=18
    const rwBackGeo = new THREE.BoxGeometry(0.1, CORRIDOR_HEIGHT, 7);
    const rwBack = new THREE.Mesh(rwBackGeo, wallMat);
    rwBack.position.set(HALF_W, 0, 14.5); // centered at z=14.5
    scene.add(rwBack);

    // back wall (end of corridor)
    const backWallGeo = new THREE.BoxGeometry(CORRIDOR_WIDTH, CORRIDOR_HEIGHT, 0.1);
    const backWall = new THREE.Mesh(backWallGeo, wallMat);
    backWall.position.set(0, 0, CORRIDOR_LENGTH);
    scene.add(backWall);

    // -- ceiling pipes --
    const pipe1Geo = new THREE.BoxGeometry(0.12, 0.12, CORRIDOR_LENGTH);
    const pipe1 = new THREE.Mesh(pipe1Geo, pipeMat);
    pipe1.position.set(-0.6, HALF_H - 0.2, CORRIDOR_LENGTH / 2);
    scene.add(pipe1);

    const pipe2Geo = new THREE.BoxGeometry(0.08, 0.08, CORRIDOR_LENGTH);
    const pipe2 = new THREE.Mesh(pipe2Geo, pipeMat);
    pipe2.position.set(0.4, HALF_H - 0.15, CORRIDOR_LENGTH / 2);
    scene.add(pipe2);

    const pipe3Geo = new THREE.BoxGeometry(0.1, 0.1, CORRIDOR_LENGTH * 0.7);
    const pipe3 = new THREE.Mesh(pipe3Geo, pipeMat);
    pipe3.position.set(-0.2, HALF_H - 0.25, CORRIDOR_LENGTH * 0.35);
    scene.add(pipe3);

    // -- bioluminescent particles --
    const PARTICLE_COUNT = 18;
    const particleGeo = new THREE.BufferGeometry();
    const particlePositions = new Float32Array(PARTICLE_COUNT * 3);
    const particleHomePositions = new Float32Array(PARTICLE_COUNT * 3);

    for (let i = 0; i < PARTICLE_COUNT; i++) {
        const idx = i * 3;
        const x = (Math.random() - 0.5) * (CORRIDOR_WIDTH - 0.4);
        const y = (Math.random() - 0.5) * (CORRIDOR_HEIGHT - 0.4);
        const z = 2 + Math.random() * (CORRIDOR_LENGTH - 3);
        particleHomePositions[idx] = x;
        particleHomePositions[idx + 1] = y;
        particleHomePositions[idx + 2] = z;
        particlePositions[idx] = x;
        particlePositions[idx + 1] = y;
        particlePositions[idx + 2] = z;
    }

    particleGeo.setAttribute('position', new THREE.BufferAttribute(particlePositions, 3));

    const particleMat = new THREE.PointsMaterial({
        color: COLORS.particle,
        size: 0.15,
        transparent: true,
        opacity: 0.7,
        depthWrite: false,
        blending: THREE.AdditiveBlending,
        sizeAttenuation: true,
    });

    const particles = new THREE.Points(particleGeo, particleMat);
    scene.add(particles);

    // -- creature shadow --
    const creatureGeo = new THREE.BoxGeometry(1.2, 2.0, 0.15);
    const creatureMat = new THREE.MeshBasicMaterial({
        color: COLORS.creature,
        transparent: true,
        opacity: 0.9,
    });
    const creature = new THREE.Mesh(creatureGeo, creatureMat);
    creature.visible = false;
    creature.position.set(0, -0.2, CORRIDOR_LENGTH - 1);
    scene.add(creature);

    // creature animation state
    let creatureActive = false;
    let creatureCrossProgress = 0;
    const CREATURE_CROSS_SPEED = 4.0; // units per second
    const CREATURE_START_X = -HALF_W - 1;
    const CREATURE_END_X = HALF_W + 1;

    // -- timing --
    const CYCLE_TIME = 9.0; // seconds per corridor traversal
    const CREATURE_TRIGGER_TIME = 3.0; // seconds into each loop
    const CAMERA_START_Z = 0.5;
    const CAMERA_END_Z = CORRIDOR_LENGTH - 1;

    // -- state --
    let degradeValue = 0;
    let loopTime = 0;
    let creatureFrequency = 1; // how many times per loop the creature can appear
    let creatureTriggerCount = 0;
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

    // -- event listeners --
    window.addEventListener('slop-degrade', function (e) {
        degradeValue = e.detail.value;
        // increase grain with degradation
        grainIntensity = 0.08 + degradeValue * 0.15;
        // creature appears more often at higher degradation
        creatureFrequency = 1 + Math.floor(degradeValue * 3);
    });

    window.addEventListener('resize', updateSize);

    // -- trigger creature cross --
    function triggerCreature() {
        if (creatureActive) return;
        creatureActive = true;
        creatureCrossProgress = 0;
        creature.visible = true;
        creature.position.x = CREATURE_START_X;
        creature.position.z = CORRIDOR_LENGTH - 1.5;
    }

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

    // -- grain update interval (throttled, not every frame) --
    let lastGrainUpdate = 0;
    const GRAIN_INTERVAL = 80; // ms

    // -- animation loop --
    function animate() {
        requestAnimationFrame(animate);

        const delta = clock.getDelta() * speedMultiplier;
        const elapsed = clock.getElapsedTime() * speedMultiplier;

        // advance loop time
        loopTime += delta;

        // grain update (throttled)
        const now = performance.now();
        if (now - lastGrainUpdate > GRAIN_INTERVAL) {
            lastGrainUpdate = now;
            updateGrain();
        }

        // -- camera position along corridor --
        const loopProgress = loopTime / CYCLE_TIME;
        const cameraZ = CAMERA_START_Z + (CAMERA_END_Z - CAMERA_START_Z) * Math.min(loopProgress, 1.0);
        camera.position.set(0, 0, cameraZ);
        camera.lookAt(0, 0, cameraZ + 5);

        // -- creature trigger(s) --
        // base trigger at CREATURE_TRIGGER_TIME; additional triggers spaced evenly
        // across the loop when degradeValue is high
        for (let t = 0; t < creatureFrequency; t++) {
            const triggerTime = CREATURE_TRIGGER_TIME + t * (CYCLE_TIME / (creatureFrequency + 1));
            // check if we just crossed this trigger time in this frame
            if (loopTime >= triggerTime && loopTime - delta < triggerTime && creatureTriggerCount <= t) {
                creatureTriggerCount = t + 1;
                triggerCreature();
            }
        }

        // -- creature cross animation --
        if (creatureActive) {
            creatureCrossProgress += CREATURE_CROSS_SPEED * delta;
            const t = creatureCrossProgress / (CREATURE_END_X - CREATURE_START_X);
            creature.position.x = CREATURE_START_X + (CREATURE_END_X - CREATURE_START_X) * Math.min(t, 1.0);

            if (t >= 1.0) {
                creature.visible = false;
                creatureActive = false;
            }
        }

        // -- bioluminescent particles: slow float --
        const posAttr = particleGeo.getAttribute('position');
        for (let i = 0; i < PARTICLE_COUNT; i++) {
            const idx = i * 3;
            const driftX = Math.sin(elapsed * 0.4 + i * 2.1) * 0.15;
            const driftY = Math.cos(elapsed * 0.3 + i * 1.7) * 0.1;
            const driftZ = Math.sin(elapsed * 0.25 + i * 3.3) * 0.08;

            posAttr.array[idx] = particleHomePositions[idx] + driftX;
            posAttr.array[idx + 1] = particleHomePositions[idx + 1] + driftY;
            posAttr.array[idx + 2] = particleHomePositions[idx + 2] + driftZ;
        }
        posAttr.needsUpdate = true;

        // particle glow pulse
        particleMat.opacity = 0.5 + Math.sin(elapsed * 1.2) * 0.2;

        // -- loop reset --
        if (loopProgress >= 1.0) {
            triggerFlash();
            loopTime = 0;
            creatureTriggerCount = 0;
        }

        renderer.render(scene, camera);
    }

    animate();
})();
