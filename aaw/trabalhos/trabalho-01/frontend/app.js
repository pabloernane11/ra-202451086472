const API_BASE = "http://localhost:5290/api/v1";

// Estado em memória da página única (SPA simples)
let modoRegistro = false;
let veiculosEmCache = [];
let veiculoSelecionado = null;
let ordensDoVeiculo = [];

// Elementos principais
const alertaEl = document.getElementById("alerta");
const barraSuperior = document.getElementById("barra-superior");
const usuarioLogadoEl = document.getElementById("usuario-logado");

const visoes = {
    auth: document.getElementById("visao-auth"),
    lista: document.getElementById("visao-lista"),
    detalhe: document.getElementById("visao-detalhe"),
    formVeiculo: document.getElementById("visao-form-veiculo"),
    formOs: document.getElementById("visao-form-os")
};

// ---------------- Utilitários ----------------

function mostrarAlerta(mensagem, tipo = "sucesso") {
    alertaEl.textContent = mensagem;
    alertaEl.className = `alerta alerta-${tipo}`;
}

function limparAlerta() {
    alertaEl.textContent = "";
    alertaEl.className = "alerta oculto";
}

function mostrarVisao(nomeVisao) {
    Object.values(visoes).forEach(secao => secao.classList.add("oculto"));
    visoes[nomeVisao].classList.remove("oculto");
}

function escaparHtml(texto) {
    return String(texto ?? "")
        .replaceAll("&", "&amp;")
        .replaceAll("<", "&lt;")
        .replaceAll(">", "&gt;")
        .replaceAll('"', "&quot;")
        .replaceAll("'", "&#039;");
}

function formatarMoeda(valor) {
    return Number(valor ?? 0).toLocaleString("pt-BR", {
        style: "currency",
        currency: "BRL"
    });
}

function formatarStatus(status) {
    const mapa = {
        Aberta: "Aberta",
        EmAndamento: "Em Andamento",
        Concluida: "Concluída",
        Cancelada: "Cancelada"
    };
    return mapa[status] ?? status;
}

function extrairMensagemErro(dados, status) {
    if (dados?.mensagem) return dados.mensagem;
    if (dados?.errors) return Object.values(dados.errors).flat().join(" ");
    if (status === 401) return "Sua sessão expirou. Faça login novamente.";
    if (status === 403) return "Você não tem permissão para excluir este registro.";
    return "Não foi possível concluir a operação. Tente novamente.";
}

function obterSessao() {
    const token = sessionStorage.getItem("oficina_token");
    const usuarioTexto = sessionStorage.getItem("oficina_usuario");
    if (!token || !usuarioTexto) return null;
    try {
        return { token, usuario: JSON.parse(usuarioTexto) };
    } catch {
        return null;
    }
}

function salvarSessao(token, usuario) {
    sessionStorage.setItem("oficina_token", token);
    sessionStorage.setItem("oficina_usuario", JSON.stringify(usuario));
}

function encerrarSessao() {
    sessionStorage.removeItem("oficina_token");
    sessionStorage.removeItem("oficina_usuario");
    barraSuperior.classList.add("oculto");
    veiculoSelecionado = null;
    mostrarVisao("auth");
}

// ---------------- Comunicação assíncrona com a API ----------------

async function requisicaoApi(caminho, opcoes = {}) {
    const sessao = obterSessao();
    const headers = {
        "Content-Type": "application/json",
        ...(opcoes.headers || {})
    };

    if (sessao?.token) {
        headers["Authorization"] = `Bearer ${sessao.token}`;
    }

    const resposta = await fetch(`${API_BASE}${caminho}`, {
        ...opcoes,
        headers
    });

    const texto = await resposta.text();
    let dados = null;
    try {
        dados = texto ? JSON.parse(texto) : null;
    } catch {
        dados = null;
    }

    if (!resposta.ok) {
        if (resposta.status === 401 && sessao?.token) {
            encerrarSessao();
        }
        throw new Error(extrairMensagemErro(dados, resposta.status));
    }

    return dados;
}

// ---------------- Autenticação (Login / Registro) ----------------

function atualizarModoAuth() {
    document.getElementById("auth-titulo").textContent = modoRegistro ? "Criar Nova Conta" : "Entrar no Sistema";
    document.getElementById("btn-auth-enviar").textContent = modoRegistro ? "Cadastrar" : "Entrar";
    document.getElementById("btn-alternar-auth").textContent = modoRegistro
        ? "Já tem conta? Fazer login"
        : "Não tem conta? Cadastre-se";

    document.getElementById("grupo-nome").classList.toggle("oculto", !modoRegistro);
    document.getElementById("grupo-role").classList.toggle("oculto", !modoRegistro);
    document.getElementById("auth-nome").required = modoRegistro;
}

document.getElementById("btn-alternar-auth").addEventListener("click", () => {
    modoRegistro = !modoRegistro;
    limparAlerta();
    atualizarModoAuth();
});

document.getElementById("form-auth").addEventListener("submit", async (evento) => {
    evento.preventDefault();
    limparAlerta();

    const email = document.getElementById("auth-email").value.trim();
    const senha = document.getElementById("auth-senha").value;

    try {
        if (modoRegistro) {
            const nome = document.getElementById("auth-nome").value.trim();
            const role = document.getElementById("auth-role").value;

            await requisicaoApi("/auth/registrar", {
                method: "POST",
                body: JSON.stringify({ nome, email, senha, role })
            });

            mostrarAlerta("Usuário cadastrado! Agora clique em Entrar.", "sucesso");
            modoRegistro = false;
            atualizarModoAuth();
            return;
        }

        const resposta = await requisicaoApi("/auth/login", {
            method: "POST",
            body: JSON.stringify({ email, senha })
        });

        salvarSessao(resposta.token, {
            nome: resposta.nome,
            email: resposta.email,
            role: resposta.role
        });

        entrarNoPainel();
    } catch (erro) {
        mostrarAlerta(erro.message, "erro");
    }
});

document.getElementById("btn-sair").addEventListener("click", () => {
    limparAlerta();
    encerrarSessao();
});

function entrarNoPainel() {
    const sessao = obterSessao();
    if (!sessao) {
        encerrarSessao();
        return;
    }

    usuarioLogadoEl.textContent = `${sessao.usuario.nome} (${sessao.usuario.role})`;
    barraSuperior.classList.remove("oculto");
    carregarVeiculos();
}

// ---------------- Visão 1: Lista de Veículos ----------------

async function carregarVeiculos() {
    mostrarVisao("lista");
    const container = document.getElementById("lista-veiculos");
    container.innerHTML = "<p class='texto-suave'>Carregando veículos...</p>";

    try {
        veiculosEmCache = await requisicaoApi("/veiculos");
        renderizarListaVeiculos();
    } catch (erro) {
        container.innerHTML = "";
        mostrarAlerta(erro.message, "erro");
    }
}

function renderizarListaVeiculos() {
    const termo = document.getElementById("filtro-busca").value.trim().toLowerCase();
    const container = document.getElementById("lista-veiculos");

    const filtrados = veiculosEmCache.filter(v =>
        `${v.placa} ${v.marca} ${v.modelo} ${v.nomeProprietario}`.toLowerCase().includes(termo)
    );

    if (filtrados.length === 0) {
        container.innerHTML = "<p class='texto-suave'>Nenhum veículo encontrado.</p>";
        return;
    }

    container.innerHTML = filtrados.map(v => `
        <article class="veiculo-card">
            <div>
                <span class="placa-tag">${escaparHtml(v.placa)}</span>
            </div>
            <h3>${escaparHtml(v.marca)} ${escaparHtml(v.modelo)} (${escaparHtml(v.ano)})</h3>
            <p class="texto-suave">Proprietário: ${escaparHtml(v.nomeProprietario)}</p>
            <div class="acoes-linha">
                <button type="button" class="btn btn-primario" data-acao="detalhes" data-id="${escaparHtml(v.id)}">
                    Ver Detalhes
                </button>
                <button type="button" class="btn btn-secundario" data-acao="editar" data-id="${escaparHtml(v.id)}">
                    Editar
                </button>
            </div>
        </article>
    `).join("");
}

document.getElementById("filtro-busca").addEventListener("input", renderizarListaVeiculos);
document.getElementById("btn-recarregar").addEventListener("click", () => {
    limparAlerta();
    carregarVeiculos();
});

document.getElementById("btn-novo-veiculo").addEventListener("click", () => {
    limparAlerta();
    abrirFormularioVeiculo();
});

document.getElementById("lista-veiculos").addEventListener("click", (evento) => {
    const botao = evento.target.closest("button[data-acao]");
    if (!botao) return;

    const id = botao.dataset.id;
    limparAlerta();

    if (botao.dataset.acao === "detalhes") {
        abrirDetalheVeiculo(id);
    } else if (botao.dataset.acao === "editar") {
        const veiculo = veiculosEmCache.find(v => v.id === id);
        if (veiculo) abrirFormularioVeiculo(veiculo);
    }
});

// ---------------- Visão 2: Detalhe do Veículo + OS ----------------

async function abrirDetalheVeiculo(veiculoId) {
    mostrarVisao("detalhe");
    const cardVeiculo = document.getElementById("detalhe-veiculo-card");
    const listaOrdensEl = document.getElementById("lista-ordens");

    cardVeiculo.innerHTML = "<p class='texto-suave'>Carregando detalhes...</p>";
    listaOrdensEl.innerHTML = "<p class='texto-suave'>Carregando ordens de serviço...</p>";

    try {
        const [veiculo, ordens] = await Promise.all([
            requisicaoApi(`/veiculos/${veiculoId}`),
            requisicaoApi(`/ordens-servico?veiculoId=${encodeURIComponent(veiculoId)}`)
        ]);

        veiculoSelecionado = veiculo;
        ordensDoVeiculo = ordens;
        renderizarDetalheVeiculo();
    } catch (erro) {
        mostrarAlerta(erro.message, "erro");
        carregarVeiculos();
    }
}

function renderizarDetalheVeiculo() {
    const v = veiculoSelecionado;
    const cardVeiculo = document.getElementById("detalhe-veiculo-card");
    const listaOrdensEl = document.getElementById("lista-ordens");

    cardVeiculo.innerHTML = `
        <div class="cabecalho-secao">
            <div>
                <span class="placa-tag">${escaparHtml(v.placa)}</span>
                <h2>${escaparHtml(v.marca)} ${escaparHtml(v.modelo)}</h2>
            </div>
            <div class="acoes-linha">
                <button id="btn-editar-veiculo" type="button" class="btn btn-secundario">Editar Veículo</button>
                <button id="btn-excluir-veiculo" type="button" class="btn btn-perigo">Excluir Veículo</button>
            </div>
        </div>
        <div class="detalhe-grid">
            <div><strong>Ano:</strong> ${escaparHtml(v.ano)}</div>
            <div><strong>Proprietário:</strong> ${escaparHtml(v.nomeProprietario)}</div>
            <div><strong>Telefone:</strong> ${escaparHtml(v.telefoneProprietario)}</div>
            <div><strong>ID:</strong> <span class="texto-suave">${escaparHtml(v.id)}</span></div>
        </div>
    `;

    document.getElementById("btn-editar-veiculo").addEventListener("click", () => {
        limparAlerta();
        abrirFormularioVeiculo(v);
    });

    document.getElementById("btn-excluir-veiculo").addEventListener("click", async () => {
        if (!confirm(`Deseja realmente excluir o veículo ${v.placa}?`)) return;
        limparAlerta();
        try {
            await requisicaoApi(`/veiculos/${v.id}`, { method: "DELETE" });
            mostrarAlerta(`Veículo ${v.placa} excluído com sucesso!`, "sucesso");
            veiculoSelecionado = null;
            carregarVeiculos();
        } catch (erro) {
            mostrarAlerta(erro.message, "erro");
        }
    });

    if (ordensDoVeiculo.length === 0) {
        listaOrdensEl.innerHTML = "<p class='texto-suave'>Nenhuma ordem de serviço cadastrada para este veículo.</p>";
        return;
    }

    listaOrdensEl.innerHTML = ordensDoVeiculo.map(os => {
        const classeStatus = `status-${String(os.status).toLowerCase()}`;
        const dataFormatada = new Date(os.dataAbertura).toLocaleString("pt-BR");

        return `
            <article class="os-card">
                <div>
                    <span class="status-badge ${classeStatus}">${escaparHtml(formatarStatus(os.status))}</span>
                    <h4 style="margin-top: 8px;">${escaparHtml(os.descricaoProblema)}</h4>
                    <p class="texto-suave">
                        Peças: ${formatarMoeda(os.valorPecas)} ·
                        Mão de obra: ${formatarMoeda(os.valorMaoDeObra)} ·
                        <strong>Total: ${formatarMoeda(os.valorTotal)}</strong>
                    </p>
                    <p class="texto-suave">Abertura: ${escaparHtml(dataFormatada)}</p>
                </div>
                <div class="acoes-linha">
                    <button type="button" class="btn btn-secundario" data-acao="editar-os" data-id="${escaparHtml(os.id)}">
                        Editar OS
                    </button>
                    <button type="button" class="btn btn-perigo" data-acao="excluir-os" data-id="${escaparHtml(os.id)}">
                        Excluir OS
                    </button>
                </div>
            </article>
        `;
    }).join("");
}

document.getElementById("btn-voltar-lista").addEventListener("click", () => {
    limparAlerta();
    veiculoSelecionado = null;
    carregarVeiculos();
});

document.getElementById("btn-nova-os").addEventListener("click", () => {
    if (!veiculoSelecionado) return;
    limparAlerta();
    abrirFormularioOs();
});

document.getElementById("lista-ordens").addEventListener("click", async (evento) => {
    const botao = evento.target.closest("button[data-acao]");
    if (!botao) return;

    const osId = botao.dataset.id;
    limparAlerta();

    if (botao.dataset.acao === "editar-os") {
        const os = ordensDoVeiculo.find(item => item.id === osId);
        if (os) abrirFormularioOs(os);
    } else if (botao.dataset.acao === "excluir-os") {
        if (!confirm("Deseja realmente excluir esta ordem de serviço?")) return;
        try {
            await requisicaoApi(`/ordens-servico/${osId}`, { method: "DELETE" });
            mostrarAlerta("Ordem de serviço excluída com sucesso!", "sucesso");
            abrirDetalheVeiculo(veiculoSelecionado.id);
        } catch (erro) {
            mostrarAlerta(erro.message, "erro");
        }
    }
});

// ---------------- Visão 3A: Formulário de Veículo ----------------

function abrirFormularioVeiculo(veiculo = null) {
    const emEdicao = Boolean(veiculo);
    document.getElementById("titulo-form-veiculo").textContent = emEdicao ? "Editar Veículo" : "Novo Veículo";
    document.getElementById("veiculo-id").value = veiculo?.id ?? "";
    document.getElementById("veiculo-placa").value = veiculo?.placa ?? "";
    document.getElementById("veiculo-marca").value = veiculo?.marca ?? "";
    document.getElementById("veiculo-modelo").value = veiculo?.modelo ?? "";
    document.getElementById("veiculo-ano").value = veiculo?.ano ?? "";
    document.getElementById("veiculo-proprietario").value = veiculo?.nomeProprietario ?? "";
    document.getElementById("veiculo-telefone").value = veiculo?.telefoneProprietario ?? "";

    mostrarVisao("formVeiculo");
}

document.getElementById("form-veiculo").addEventListener("submit", async (evento) => {
    evento.preventDefault();
    limparAlerta();

    const id = document.getElementById("veiculo-id").value;
    const payload = {
        placa: document.getElementById("veiculo-placa").value.trim(),
        marca: document.getElementById("veiculo-marca").value.trim(),
        modelo: document.getElementById("veiculo-modelo").value.trim(),
        ano: Number(document.getElementById("veiculo-ano").value),
        nomeProprietario: document.getElementById("veiculo-proprietario").value.trim(),
        telefoneProprietario: document.getElementById("veiculo-telefone").value.trim()
    };

    try {
        if (id) {
            await requisicaoApi(`/veiculos/${id}`, {
                method: "PUT",
                body: JSON.stringify({ id, ...payload })
            });
            mostrarAlerta("Veículo atualizado com sucesso!", "sucesso");
            abrirDetalheVeiculo(id);
        } else {
            const criado = await requisicaoApi("/veiculos", {
                method: "POST",
                body: JSON.stringify(payload)
            });
            mostrarAlerta("Veículo cadastrado com sucesso!", "sucesso");
            abrirDetalheVeiculo(criado.id);
        }
    } catch (erro) {
        mostrarAlerta(erro.message, "erro");
    }
});

document.getElementById("btn-cancelar-veiculo").addEventListener("click", () => {
    limparAlerta();
    if (veiculoSelecionado) {
        abrirDetalheVeiculo(veiculoSelecionado.id);
    } else {
        carregarVeiculos();
    }
});

// ---------------- Visão 3B: Formulário de Ordem de Serviço ----------------

function abrirFormularioOs(os = null) {
    const emEdicao = Boolean(os);
    document.getElementById("titulo-form-os").textContent = emEdicao
        ? "Editar Ordem de Serviço"
        : "Nova Ordem de Serviço";

    document.getElementById("os-veiculo-info").textContent =
        `Veículo: ${veiculoSelecionado.placa} — ${veiculoSelecionado.marca} ${veiculoSelecionado.modelo}`;

    document.getElementById("os-id").value = os?.id ?? "";
    document.getElementById("os-veiculo-id").value = veiculoSelecionado.id;
    document.getElementById("os-descricao").value = os?.descricaoProblema ?? "";
    document.getElementById("os-pecas").value = os?.valorPecas ?? 0;
    document.getElementById("os-mao-obra").value = os?.valorMaoDeObra ?? 0;

    const grupoStatus = document.getElementById("grupo-os-status");
    const selectStatus = document.getElementById("os-status");
    if (emEdicao) {
        grupoStatus.classList.remove("oculto");
        selectStatus.value = os.status;
    } else {
        grupoStatus.classList.add("oculto");
        selectStatus.value = "Aberta";
    }

    mostrarVisao("formOs");
}

document.getElementById("form-os").addEventListener("submit", async (evento) => {
    evento.preventDefault();
    limparAlerta();

    const id = document.getElementById("os-id").value;
    const veiculoId = document.getElementById("os-veiculo-id").value;
    const payload = {
        veiculoId,
        descricaoProblema: document.getElementById("os-descricao").value.trim(),
        valorPecas: parseFloat(document.getElementById("os-pecas").value) || 0,
        valorMaoDeObra: parseFloat(document.getElementById("os-mao-obra").value) || 0
    };

    try {
        if (id) {
            const status = document.getElementById("os-status").value;
            await requisicaoApi(`/ordens-servico/${id}`, {
                method: "PUT",
                body: JSON.stringify({ id, ...payload, status })
            });
            mostrarAlerta("Ordem de serviço atualizada com sucesso!", "sucesso");
        } else {
            await requisicaoApi("/ordens-servico", {
                method: "POST",
                body: JSON.stringify(payload)
            });
            mostrarAlerta("Ordem de serviço aberta com sucesso!", "sucesso");
        }

        abrirDetalheVeiculo(veiculoId);
    } catch (erro) {
        mostrarAlerta(erro.message, "erro");
    }
});

document.getElementById("btn-cancelar-os").addEventListener("click", () => {
    limparAlerta();
    if (veiculoSelecionado) {
        abrirDetalheVeiculo(veiculoSelecionado.id);
    } else {
        carregarVeiculos();
    }
});

// ---------------- Inicialização ----------------
entrarNoPainel();