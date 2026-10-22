// Comportamentos comuns aos dois sites, todos ligados por atributo data-*, sem framework.
window.Lawllit = (function () {
    var svg = 'http://www.w3.org/2000/svg';

    function icone(caminho) {
        var elemento = document.createElementNS(svg, 'svg');
        elemento.setAttribute('class', 'icone');
        elemento.setAttribute('viewBox', '0 0 24 24');
        elemento.setAttribute('aria-hidden', 'true');

        var traco = document.createElementNS(svg, 'path');
        traco.setAttribute('d', caminho);
        elemento.appendChild(traco);

        return elemento;
    }

    // Cor nunca sozinha: o aviso de erro e o de sucesso também diferem no ícone.
    function avisar(texto, tipo) {
        var pilha = document.querySelector('.avisos');

        if (!pilha) {
            pilha = document.createElement('div');
            pilha.className = 'avisos';
            pilha.setAttribute('role', 'status');
            pilha.setAttribute('aria-live', 'polite');
            document.body.appendChild(pilha);
        }

        var erro = tipo === 'erro';
        var aviso = document.createElement('div');
        aviso.className = 'aviso ' + (erro ? 'aviso-erro' : 'aviso-ok');
        aviso.appendChild(icone(erro ? 'M12 8v5M12 16h.01M10.3 3.9 1.8 18a2 2 0 0 0 1.7 3h17a2 2 0 0 0 1.7-3L13.7 3.9a2 2 0 0 0-3.4 0z' : 'M20 6 9 17l-5-5'));

        var corpo = document.createElement('span');
        corpo.textContent = texto;
        aviso.appendChild(corpo);

        pilha.appendChild(aviso);
        setTimeout(function () { aviso.remove(); }, erro ? 6000 : 3500);
    }

    // POST com o token antiforgery e o cabeçalho de AJAX, para a sessão vencida voltar 401
    // em vez da tela de login inteira dentro da resposta.
    function enviar(url, dados) {
        if (!navigator.onLine) {
            avisar('Sem internet. Confira a conexão e tente de novo.', 'erro');
            return Promise.reject(new Error('offline'));
        }

        var corpo = dados instanceof FormData ? dados : new FormData();
        var token = document.querySelector('input[name="__RequestVerificationToken"]');

        if (!(dados instanceof FormData) && dados) {
            Object.keys(dados).forEach(function (campo) { corpo.append(campo, dados[campo]); });
        }

        return fetch(url, {
            method: 'POST',
            body: corpo,
            headers: {
                'X-Requested-With': 'XMLHttpRequest',
                'RequestVerificationToken': token ? token.value : ''
            }
        }).then(function (resposta) {
            if (resposta.status === 401) {
                var entrar = document.body.getAttribute('data-url-entrar');
                if (entrar) window.location.href = entrar;
                throw new Error('sessao');
            }

            return resposta;
        });
    }

    function aplicarMedidas(raiz) {
        // A CSP não aceita style inline, então a medida vem em data-* e entra pelo CSSOM.
        raiz.querySelectorAll('[data-largura]').forEach(function (elemento) {
            elemento.style.width = Math.min(Number(elemento.getAttribute('data-largura')) || 0, 100) + '%';
        });

        raiz.querySelectorAll('[data-altura]').forEach(function (elemento) {
            elemento.style.height = Math.min(Number(elemento.getAttribute('data-altura')) || 0, 100) + '%';
        });
    }

    function revelar() {
        var pendentes = [];

        document.querySelectorAll('[data-revelar]').forEach(function (alvo) {
            if (alvo.getBoundingClientRect().top > window.innerHeight * 0.9) {
                alvo.classList.add('oculto');
                pendentes.push(alvo);
            }
        });

        if (!pendentes.length || !('IntersectionObserver' in window)) {
            pendentes.forEach(function (alvo) { alvo.classList.remove('oculto'); });
            return;
        }

        var observador = new IntersectionObserver(function (entradas) {
            entradas.forEach(function (entrada) {
                if (!entrada.isIntersecting) return;

                entrada.target.classList.remove('oculto');
                observador.unobserve(entrada.target);
            });
        }, { threshold: 0.12 });

        pendentes.forEach(function (alvo) { observador.observe(alvo); });
    }

    function copiar(botao, texto) {
        if (!texto) return;

        navigator.clipboard.writeText(texto).then(function () {
            var rotulo = botao.getAttribute('aria-label') || botao.textContent;
            botao.classList.add('copiado');
            avisar('Copiado.');
            setTimeout(function () { botao.classList.remove('copiado'); botao.setAttribute('aria-label', rotulo); }, 1200);
        });
    }

    document.addEventListener('click', function (evento) {
        var abrir = evento.target.closest('[data-abrir]');
        if (abrir) {
            var gaveta = document.getElementById(abrir.getAttribute('data-abrir'));
            if (gaveta && gaveta.showModal) gaveta.showModal();
            return;
        }

        var fechar = evento.target.closest('[data-fechar]');
        if (fechar) {
            var aberta = fechar.closest('dialog');
            if (aberta) aberta.close();
            return;
        }

        // Toque fora do conteúdo da gaveta cai no próprio dialog, que é o fundo escurecido.
        if (evento.target.tagName === 'DIALOG' && evento.target.open) {
            evento.target.close();
            return;
        }

        var copia = evento.target.closest('[data-copiar]');
        if (copia) {
            var origem = document.querySelector(copia.getAttribute('data-copiar'));
            if (origem) copiar(copia, 'value' in origem ? origem.value : origem.textContent);
        }
    });

    document.addEventListener('DOMContentLoaded', function () {
        aplicarMedidas(document);
        revelar();

        // Aviso que veio pronto do servidor some no mesmo tempo do que nasce em JS.
        document.querySelectorAll('.aviso').forEach(function (aviso) {
            setTimeout(function () { aviso.remove(); }, aviso.classList.contains('aviso-erro') ? 6000 : 3500);
        });
    });

    return { avisar: avisar, enviar: enviar, copiar: copiar };
})();
