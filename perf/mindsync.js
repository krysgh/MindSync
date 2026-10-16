import http from 'k6/http';
import { check, sleep } from 'k6';
import { Trend } from 'k6/metrics';

const BASE_URL = (__ENV.BASE_URL || 'http://host.docker.internal:5000').replace(/\/$/, '');
const API = `${BASE_URL}/api`;
const PASSWORD = 'Perf#Test12345';
const RUN_ID = __ENV.RUN_ID || String(Date.now());
const PEAK_VUS = Number(__ENV.PEAK_VUS || 10);
const BASELINE_ITERATIONS = Number(__ENV.ITERATIONS || 100);
const IDOR_ITERATIONS = Number(__ENV.IDOR_ITERATIONS || 30);
const THINK_TIME = Number(__ENV.THINK_TIME || 0.5);
const DOWNLOAD_AUDIO = (__ENV.DOWNLOAD_AUDIO || '1') !== '0';
const ENABLED = (__ENV.SCENARIOS || 'baseline').split(',').map((s) => s.trim());

const FREQUENCIES = [
    'Delta (2 Hz) - Sono Profundo',
    'Theta (6 Hz) - Meditação',
    'Alpha (10 Hz) - Relaxamento',
    'Beta (20 Hz) - Foco e Atenção',
];

const ENDPOINTS = [
    'login',
    'create_session',
    'resolve_mix',
    ...(DOWNLOAD_AUDIO ? ['audio_download'] : []),
    'complete_session',
    'list_sessions',
];

const audioReady = new Trend('audio_ready_ms', true);

const allScenarios = {
    baseline: {
        executor: 'per-vu-iterations',
        vus: 1,
        iterations: BASELINE_ITERATIONS,
        maxDuration: '15m',
        exec: 'journey',
    },
    load: {
        executor: 'ramping-vus',
        startVUs: 0,
        stages: [
            { duration: '30s', target: PEAK_VUS },
            { duration: '2m', target: PEAK_VUS },
            { duration: '15s', target: 0 },
        ],
        gracefulRampDown: '20s',
        exec: 'journey',
    },
    idor: {
        executor: 'per-vu-iterations',
        vus: 1,
        iterations: IDOR_ITERATIONS,
        maxDuration: '10m',
        exec: 'idor',
    },
};

const thresholds = {};
ENABLED.forEach((name) => {
    if (name === 'idor') {
        thresholds['checks{scenario:idor}'] = ['rate==1'];
        return;
    }
    thresholds[`http_req_failed{scenario:${name}}`] = ['rate<0.01'];
    if (DOWNLOAD_AUDIO) thresholds[`audio_ready_ms{scenario:${name}}`] = ['max>=0'];
    ENDPOINTS.forEach((endpoint) => {
        thresholds[`http_req_duration{scenario:${name},endpoint:${endpoint}}`] = ['max>=0'];
    });
});

export const options = {
    scenarios: Object.fromEntries(ENABLED.map((name) => [name, allScenarios[name]])),
    thresholds,
    setupTimeout: '180s',
    noCookiesReset: true,
    summaryTrendStats: ['min', 'med', 'p(90)', 'p(95)', 'p(99)', 'max', 'avg'],
};

function request(method, path, body, endpoint, jar) {
    const params = {
        headers: { 'Content-Type': 'application/json' },
        tags: { endpoint },
    };
    if (jar) params.jar = jar;
    return http.request(method, `${API}${path}`, body ? JSON.stringify(body) : null, params);
}

function login(user, jar) {
    const response = request('POST', '/auth/login', { email: user.email, password: PASSWORD }, 'login', jar);
    return response.status === 200;
}

function randomInt(min, max) {
    return Math.floor(Math.random() * (max - min + 1)) + min;
}

function randomGuid() {
    return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, (c) => {
        const r = (Math.random() * 16) | 0;
        return (c === 'x' ? r : (r & 0x3) | 0x8).toString(16);
    });
}

function toLocalUrl(audioUrl) {
    return audioUrl.replace(/^https?:\/\/[^/]+/, BASE_URL);
}

function safeJson(response, selector) {
    try {
        return response.json(selector);
    } catch (e) {
        return undefined;
    }
}

export function setup() {
    const count = Math.max(PEAK_VUS, 2);
    const users = [];
    for (let i = 0; i < count; i++) {
        const email = `perf_${RUN_ID}_${i}@example.com`;
        const response = http.post(
            `${API}/auth/register`,
            JSON.stringify({ firstName: 'Perf', lastName: `User${i}`, email, password: PASSWORD }),
            { headers: { 'Content-Type': 'application/json' }, tags: { endpoint: 'setup_register' } }
        );
        if (response.status !== 201) {
            throw new Error(`Could not register ${email}: ${response.status} ${response.body}`);
        }
        users.push({ email });
    }
    return { users };
}

let loggedIn = false;

export function journey(data) {
    const user = data.users[(__VU - 1) % data.users.length];

    if (!loggedIn) {
        loggedIn = login(user);
        check(null, { 'login succeeded': () => loggedIn });
        if (!loggedIn) return;
    }

    const before = randomInt(3, 8);
    const created = request(
        'POST',
        '/sessions',
        { stressLevelBefore: before, targetFrequency: FREQUENCIES[randomInt(0, FREQUENCIES.length - 1)] },
        'create_session'
    );
    const sessionId = safeJson(created, 'data');
    const createdOk = check(created, { 'create session returned 200': (r) => r.status === 200 }) && !!sessionId;
    if (!createdOk) return;

    const mix = request('POST', `/mixes/sessions/${sessionId}`, null, 'resolve_mix');
    const layers = safeJson(mix, 'data.layers') || [];
    const mixOk = check(mix, { 'resolve mix returned 200': (r) => r.status === 200 }) && layers.length > 0;
    if (!mixOk) return;

    if (DOWNLOAD_AUDIO) {
        const downloads = http.batch(
            layers.map((layer) => ({
                method: 'GET',
                url: toLocalUrl(layer.audioUrl),
                params: { tags: { endpoint: 'audio_download' }, responseType: 'none' },
            }))
        );
        check(downloads, { 'all audio layers returned 200': (rs) => rs.every((r) => r.status === 200) });

        const slowestDownload = Math.max(...downloads.map((r) => r.timings.duration));
        audioReady.add(created.timings.duration + mix.timings.duration + slowestDownload);
    }

    const completed = request(
        'PATCH',
        `/sessions/${sessionId}/complete`,
        { stressLevelAfter: Math.max(0, before - randomInt(0, 3)) },
        'complete_session'
    );
    check(completed, { 'complete session returned 200': (r) => r.status === 200 });

    const listed = request('GET', '/sessions/me', null, 'list_sessions');
    const sessions = safeJson(listed, 'data') || [];
    check(listed, {
        'list returned 200': (r) => r.status === 200,
        'list contains the created session': () => sessions.some((s) => s.id === sessionId),
    });

    sleep(THINK_TIME);
}

export function idor(data) {
    const jarA = new http.CookieJar();
    const jarB = new http.CookieJar();
    const jarAnonymous = new http.CookieJar();

    const loggedA = login(data.users[0], jarA);
    const loggedB = login(data.users[1], jarB);
    if (!check(null, { 'both users logged in': () => loggedA && loggedB })) return;

    const created = request(
        'POST',
        '/sessions',
        { stressLevelBefore: 6, targetFrequency: FREQUENCIES[2] },
        'create_session',
        jarA
    );
    const sessionId = safeJson(created, 'data');
    if (!check(created, { 'owner created a session': (r) => r.status === 200 }) || !sessionId) return;

    const foreignComplete = request('PATCH', `/sessions/${sessionId}/complete`, { stressLevelAfter: 1 }, 'complete_session', jarB);
    check(foreignComplete, { 'foreign complete returns 404': (r) => r.status === 404 });

    const missingComplete = request('PATCH', `/sessions/${randomGuid()}/complete`, { stressLevelAfter: 1 }, 'complete_session', jarA);
    check(missingComplete, {
        'nonexistent complete returns 404': (r) => r.status === 404,
        'foreign and nonexistent are indistinguishable': () =>
            safeJson(foreignComplete, 'message') === safeJson(missingComplete, 'message'),
    });

    const foreignMixBefore = request('POST', `/mixes/sessions/${sessionId}`, null, 'resolve_mix', jarB);
    check(foreignMixBefore, { 'foreign mix before owner resolved returns 404': (r) => r.status === 404 });

    const ownerMix = request('POST', `/mixes/sessions/${sessionId}`, null, 'resolve_mix', jarA);
    check(ownerMix, { 'owner mix returns 200': (r) => r.status === 200 });

    const foreignMixAfter = request('POST', `/mixes/sessions/${sessionId}`, null, 'resolve_mix', jarB);
    check(foreignMixAfter, { 'foreign mix after owner resolved returns 404': (r) => r.status === 404 });

    const foreignList = request('GET', '/sessions/me', null, 'list_sessions', jarB);
    const foreignSessions = safeJson(foreignList, 'data') || [];
    check(foreignList, {
        'foreign list returns 200': (r) => r.status === 200,
        'foreign list does not contain the session': () => !foreignSessions.some((s) => s.id === sessionId),
    });

    const anonymousComplete = request('PATCH', `/sessions/${sessionId}/complete`, { stressLevelAfter: 1 }, 'complete_session', jarAnonymous);
    check(anonymousComplete, { 'anonymous complete returns 401': (r) => r.status === 401 });

    const ownerList = request('GET', '/sessions/me', null, 'list_sessions', jarA);
    const ownerSessions = safeJson(ownerList, 'data') || [];
    const stored = ownerSessions.find((s) => s.id === sessionId);
    check(ownerList, {
        'session untouched after attacks': () => !!stored && (stored.stressLevelAfter === null || stored.stressLevelAfter === undefined),
    });

    const ownerComplete = request('PATCH', `/sessions/${sessionId}/complete`, { stressLevelAfter: 2 }, 'complete_session', jarA);
    check(ownerComplete, { 'owner complete returns 200 (positive control)': (r) => r.status === 200 });
}
