(function () {
    'use strict';

    // ── Smooth scroll (all pages) ──────────────────────
    document.addEventListener('click', function (e) {
        var link = e.target.closest('a[href^="#"]');
        if (!link) return;
        var target = document.querySelector(link.getAttribute('href'));
        if (!target) return;
        e.preventDefault();
        var nav = document.getElementById('main-nav');
        var offset = nav ? nav.offsetHeight + 8 : 0;
        window.scrollTo({ top: target.getBoundingClientRect().top + window.scrollY - offset, behavior: 'smooth' });
    });

    // ── Terminal (homepage only) ───────────────────────
    var win     = document.getElementById('termWindow');
    var output  = document.getElementById('termOutput');
    var display = document.getElementById('termDisplay');
    var input   = document.getElementById('termInput');
    if (!win || !output || !input) return;

    // ── Helpers ────────────────────────────────────────
    function ln(html) {
        var p = document.createElement('p');
        p.innerHTML = html;
        output.appendChild(p);
        output.scrollTop = output.scrollHeight;
    }

    function blank() { ln('&nbsp;'); }

    function wait(ms) {
        return new Promise(function (r) { setTimeout(r, ms); });
    }

    function esc(s) {
        return String(s)
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;');
    }

    function prompt(cmd) {
        ln('<span class="tp">$</span> ' + esc(cmd));
    }

    // ── Intro animation ────────────────────────────────
    async function intro() {
        input.disabled = true;

        await wait(350);
        prompt('dotnet build Lawllit.Web --configuration Release');
        await wait(500);
        ln('&nbsp;&nbsp;lawllit <span class="td">·</span> we build software. <span class="td">that actually works.</span>');
        ln('&nbsp;&nbsp;<span class="td">clean code, systems that work, no bullshit.</span>');
        await wait(650);
        ln('&nbsp;&nbsp;<span class="td">Restore succeeded.</span>');
        ln('&nbsp;&nbsp;<span class="td">Build succeeded. <span class="tok">✓</span> 0 Warning(s) · 0 Error(s)</span>');
        blank();

        await wait(750);
        prompt('dotnet run --project Lawllit.Finance');
        await wait(600);
        ln('&nbsp;&nbsp;<span class="td">info: Application started.</span>');
        ln('&nbsp;&nbsp;<span class="td">info: Now listening on https://lawllit.com</span>');
        blank();

        await wait(450);
        ln('&nbsp;&nbsp;<a href="/Finance" class="tlink"><span class="tdot"></span>live · lawllit.finance &nbsp;→</a>');
        blank();

        input.disabled = false;
        input.focus();
    }

    intro();

    // ── Focus on click ─────────────────────────────────
    win.addEventListener('click', function () { input.focus(); });

    // ── Sync display ───────────────────────────────────
    input.addEventListener('input', function () {
        display.textContent = input.value;
    });

    // ── Handle Enter ──────────────────────────────────
    input.addEventListener('keydown', function (e) {
        if (e.key !== 'Enter') return;
        e.preventDefault();
        var cmd = input.value.trim();
        input.value = '';
        display.textContent = '';
        if (!input.disabled) run(cmd);
    });

    // ── Command dispatcher ─────────────────────────────
    async function run(cmd) {
        input.disabled = true;
        prompt(cmd);

        var c = cmd.toLowerCase();

        if (c === '') {
            // noop
        } else if (c === 'clear') {
            output.innerHTML = '';
        } else if (c === 'help' || c === '--help' || c === '-h') {
            cmdHelp();
        } else if (c === 'ls' || c === 'dir') {
            cmdLs();
        } else if (c === 'whoami') {
            ln('&nbsp;&nbsp;lawllit');
        } else if (isFinanceCmd(c)) {
            await cmdFinance();
        } else if (isNukeCmd(c)) {
            await cmdNuke();
        } else {
            ln('&nbsp;&nbsp;<span class="td">zsh: command not found: ' + esc(cmd) + '</span>');
        }

        input.disabled = false;
        input.focus();
    }

    function isFinanceCmd(c) {
        return (c.includes('dotnet') && c.includes('finance')) ||
               c === 'finance' ||
               c === 'run finance';
    }

    function isNukeCmd(c) {
        return /rm\s+-rf/.test(c) ||
               /del\s+\/[fsq]/i.test(c) ||
               /rmdir\s+\/s/i.test(c) ||
               (c.includes('dotnet') && /delete|remove|nuke|destroy/.test(c)) ||
               c === 'nuke';
    }

    // ── Commands ───────────────────────────────────────
    function cmdHelp() {
        blank();
        ln('&nbsp;&nbsp;available commands:');
        ln('&nbsp;&nbsp;');
        ln('&nbsp;&nbsp;  <span class="td">ls</span>                          list project files');
        ln('&nbsp;&nbsp;  <span class="td">dotnet run --project Lawllit.Finance</span>');
        ln('&nbsp;&nbsp;                               launch finance app');
        ln('&nbsp;&nbsp;  <span class="td">whoami</span>                      current user');
        ln('&nbsp;&nbsp;  <span class="td">clear</span>                       clear terminal');
        blank();
    }

    function cmdLs() {
        blank();
        ln('&nbsp;&nbsp;Lawllit.Web/&nbsp;&nbsp;&nbsp;&nbsp;Lawllit.Finance/&nbsp;&nbsp;&nbsp;&nbsp;Lawllit.Api/&nbsp;&nbsp;&nbsp;&nbsp;README.md');
        blank();
    }

    async function cmdFinance() {
        blank();
        ln('&nbsp;&nbsp;<span class="td">Building...</span>');
        await wait(700);
        ln('&nbsp;&nbsp;<span class="td">Restore succeeded.</span>');
        ln('&nbsp;&nbsp;<span class="td">Build succeeded. <span class="tok">✓</span></span>');
        await wait(500);
        ln('&nbsp;&nbsp;<span class="td">info: Application started.</span>');
        ln('&nbsp;&nbsp;<span class="td">info: Now listening on https://lawllit.com</span>');
        blank();
        ln('&nbsp;&nbsp;<a href="/Finance" class="tlink"><span class="tdot"></span>live · lawllit.finance &nbsp;→</a>');
        blank();
        await wait(1200);
        window.location.href = '/Finance';
    }

    async function cmdNuke() {
        blank();
        ln('&nbsp;&nbsp;<span class="twarn">⚠  this action is irreversible.</span>');
        await wait(1000);
        ln('&nbsp;&nbsp;removing Controllers/ ... <span class="tok">✓</span>');
        await wait(380);
        ln('&nbsp;&nbsp;removing Views/ ... <span class="tok">✓</span>');
        await wait(320);
        ln('&nbsp;&nbsp;removing wwwroot/ ... <span class="tok">✓</span>');
        await wait(290);
        ln('&nbsp;&nbsp;removing Program.cs ... <span class="tok">✓</span>');
        await wait(260);
        ln('&nbsp;&nbsp;removing appsettings.json ... <span class="tok">✓</span>');
        await wait(600);
        blank();
        ln('&nbsp;&nbsp;47 files removed. Lawllit.Web has been deleted.');
        await wait(1800);
        blank();
        ln('&nbsp;&nbsp;...');
        await wait(1100);
        blank();
        ln('&nbsp;&nbsp;just kidding xD');
        await wait(350);
        ln('&nbsp;&nbsp;<span class="td">lawllit.web is still alive and kicking.</span>');
        blank();
    }
})();
