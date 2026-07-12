(function () {
    'use strict';

    const pacienteId = parseInt(document.getElementById('evolPacienteId')?.value || '0', 10);
    if (!pacienteId) return;

    let chartIctStatus = null;
    let chartPA = null, chartGlicemia = null, chartPeso = null, chartContadores = null;
    let detalhesRenderizados = false;
    let evolucoesCache = [];
    let serieCache = null;

    const STATUS_CORES = {
        0: '#6c757d', // Inconclusivo - cinza
        1: '#198754', // Melhorou - verde
        2: '#ffc107', // Estável - amarelo
        3: '#dc3545'  // Piorou - vermelho
    };
    const STATUS_LABELS = {
        0: 'Inconclusivo', 1: 'Melhorou', 2: 'Estável', 3: 'Piorou'
    };

    const modalEvol = new bootstrap.Modal(document.getElementById('modalEvolucao'));

    // ============ Carregamento da aba (lazy) ============
    document.getElementById('tab-evolucoes-btn')?.addEventListener('shown.bs.tab', carregarEvolucoes);
    document.getElementById('btnRecarregarEvol')?.addEventListener('click', carregarEvolucoes);

    let jaCarregou = false;
    async function carregarEvolucoes() {
        try {
            const [listaResp, serieResp] = await Promise.all([
                fetch(`/EvolucaoClinica/ListarPorPaciente?pacienteId=${pacienteId}`).then(r => r.json()),
                fetch(`/EvolucaoClinica/SerieTemporal?pacienteId=${pacienteId}`).then(r => r.json())
            ]);

            if (!listaResp.success) {
                Swal.fire('Erro', listaResp.message || 'Falha ao carregar evoluções.', 'error');
                return;
            }
            evolucoesCache = listaResp.data || [];
            serieCache = serieResp.success ? serieResp.data : null;

            if (evolucoesCache.length === 0) {
                document.getElementById('evolEmptyState').style.display = '';
                document.getElementById('evolConteudo').style.display = 'none';
                return;
            }

            document.getElementById('evolEmptyState').style.display = 'none';
            document.getElementById('evolConteudo').style.display = '';
            renderizarTabela(evolucoesCache);
            renderizarGraficoPrincipal(serieCache);
            detalhesRenderizados = false;

            jaCarregou = true;
        } catch (e) {
            Swal.fire('Erro', 'Falha de comunicação com o servidor.', 'error');
        }
    }

    // Stubs — implementados em tasks 21+
    function renderizarTabela(lista) {
        const tbody = document.querySelector('#tabelaEvolucoes tbody');
        tbody.innerHTML = lista.map(e => {
            const data = new Date(e.dataConsulta).toLocaleDateString('pt-BR');
            const cor = STATUS_CORES[e.status] || '#6c757d';
            const cidTitulo = (e.cidTitulo || '').replace(/^[\-\s]+/, '');
            const cid = e.cidCodigo
                ? `<span class="evol-cid-titulo">${cidTitulo}</span><br><span class="evol-cid-code">${e.cidCodigo}</span>`
                : '<span class="text-muted">—</span>';
            const prescricao = e.prescricaoID
                ? `Prescrição #${e.prescricaoID} — ICT ${e.ictDaPrescricao?.toFixed(2) ?? '—'}`
                : '<span class="text-muted">Sem prescrição</span>';
            return `
                <tr>
                    <td>${data}</td>
                    <td><span class="badge" style="background:${cor}">${e.statusLabel}</span></td>
                    <td>${cid}</td>
                    <td>${prescricao}</td>
                    <td class="text-end">
                        <button class="btn btn-sm btn-outline-secondary btn-ver-evol" data-id="${e.id}" title="Ver">
                            <i class="bi bi-eye"></i>
                        </button>
                        <button class="btn btn-sm btn-outline-primary btn-editar-evol" data-id="${e.id}" title="Editar">
                            <i class="bi bi-pencil"></i>
                        </button>
                        <button class="btn btn-sm btn-outline-danger btn-deletar-evol" data-id="${e.id}" title="Deletar">
                            <i class="bi bi-trash"></i>
                        </button>
                    </td>
                </tr>`;
        }).join('');
    }
    function renderizarGraficoPrincipal(serie) {
        if (!serie || serie.datas.length === 0) return;
        const ctx = document.getElementById('chartIctStatus').getContext('2d');
        const labels = serie.datas.map(d => new Date(d).toLocaleDateString('pt-BR'));
        const cores = serie.statusPorEvolucao.map(s => STATUS_CORES[s] || '#6c757d');
        const statusLabels = serie.statusPorEvolucao.map(s => STATUS_LABELS[s] || '—');

        if (chartIctStatus) chartIctStatus.destroy();
        chartIctStatus = new Chart(ctx, {
            type: 'line',
            data: {
                labels,
                datasets: [{
                    label: 'ICT',
                    data: serie.ictPorEvolucao,
                    borderColor: '#0d6efd',
                    backgroundColor: 'rgba(13,110,253,0.1)',
                    pointBackgroundColor: cores,
                    pointBorderColor: cores,
                    pointRadius: 8,
                    pointHoverRadius: 10,
                    spanGaps: false,
                    tension: 0.2
                }]
            },
            options: {
                responsive: true,
                plugins: {
                    tooltip: {
                        callbacks: {
                            label: (ctx) => {
                                const ict = ctx.parsed.y;
                                const st = statusLabels[ctx.dataIndex];
                                return `ICT: ${ict ?? '—'} — Status: ${st}`;
                            }
                        }
                    },
                    legend: { display: false }
                },
                scales: {
                    y: { title: { display: true, text: 'ICT (complexidade)' } }
                }
            }
        });
    }

    function renderizarGraficosDetalhados(serie) {
        if (!serie || serie.datas.length === 0) return;
        const labels = serie.datas.map(d => new Date(d).toLocaleDateString('pt-BR'));

        // 1. PA + FC
        if (chartPA) chartPA.destroy();
        chartPA = new Chart(document.getElementById('chartPA').getContext('2d'), {
            type: 'line',
            data: {
                labels,
                datasets: [
                    { label: 'PA Sistólica', data: serie.paSistolica, borderColor: '#dc3545', spanGaps: false, tension: 0.2 },
                    { label: 'PA Diastólica', data: serie.paDiastolica, borderColor: '#fd7e14', spanGaps: false, tension: 0.2 },
                    { label: 'FC', data: serie.frequenciaCardiaca, borderColor: '#0dcaf0', spanGaps: false, tension: 0.2 }
                ]
            },
            options: { responsive: true }
        });

        // 2. Glicemia + HbA1c
        if (chartGlicemia) chartGlicemia.destroy();
        chartGlicemia = new Chart(document.getElementById('chartGlicemia').getContext('2d'), {
            type: 'line',
            data: {
                labels,
                datasets: [
                    { label: 'Glicemia', data: serie.glicemia, borderColor: '#6610f2', spanGaps: false, tension: 0.2, yAxisID: 'y' },
                    { label: 'HbA1c (%)', data: serie.hba1c, borderColor: '#198754', spanGaps: false, tension: 0.2, yAxisID: 'y1' }
                ]
            },
            options: {
                responsive: true,
                scales: {
                    y: { type: 'linear', position: 'left', title: { display: true, text: 'Glicemia mg/dL' } },
                    y1: { type: 'linear', position: 'right', title: { display: true, text: 'HbA1c %' }, grid: { drawOnChartArea: false } }
                }
            }
        });

        // 3. Peso
        if (chartPeso) chartPeso.destroy();
        chartPeso = new Chart(document.getElementById('chartPeso').getContext('2d'), {
            type: 'line',
            data: {
                labels,
                datasets: [{ label: 'Peso (kg)', data: serie.peso, borderColor: '#6f42c1', spanGaps: false, tension: 0.2 }]
            },
            options: { responsive: true }
        });

        // 4. Contadores
        if (chartContadores) chartContadores.destroy();
        chartContadores = new Chart(document.getElementById('chartContadores').getContext('2d'), {
            type: 'bar',
            data: {
                labels,
                datasets: [
                    { label: 'Eventos Adversos', data: serie.eventosAdversos, backgroundColor: '#ffc107' },
                    { label: 'Hospitalizações', data: serie.hospitalizacoes, backgroundColor: '#dc3545' },
                    { label: 'Emergências', data: serie.idasEmergencia, backgroundColor: '#fd7e14' }
                ]
            },
            options: { responsive: true, scales: { y: { beginAtZero: true } } }
        });
    }

    // ============ Query string: abrir modal direto vindo de Form.cshtml ============
    document.addEventListener('DOMContentLoaded', () => {
        const params = new URLSearchParams(window.location.search);
        if (params.get('abrirEvolucao') === '1') {
            const tabBtn = document.getElementById('tab-evolucoes-btn');
            if (tabBtn) bootstrap.Tab.getOrCreateInstance(tabBtn).show();
            setTimeout(() => {
                const prescricaoId = parseInt(params.get('prescricaoId') || '0', 10) || null;
                abrirModalEvolucao({ modo: 'criar', prescricaoIdPreSel: prescricaoId });
            }, 400);
        }
    });

    // Stub — implementado em Task 22
    async function abrirModalEvolucao({ modo, id, prescricaoIdPreSel, cidPreSel } = {}) {
        // Resetar form
        document.getElementById('formEvolucao').reset();
        document.getElementById('evolucaoId').value = '';

        const tituloEl = document.getElementById('modalEvolucaoTituloTxt');
        const btnSalvar = document.getElementById('btnSalvarEvolucao');

        // Carregar selects (receitas + CIDs do paciente) sempre que abrir
        await carregarSelectsModal();

        if (modo === 'criar') {
            tituloEl.textContent = 'Nova Evolução Clínica';
            btnSalvar.style.display = '';
            habilitarCampos(true);
            document.getElementById('evolDataConsulta').value = new Date().toISOString().substring(0, 10);
            if (prescricaoIdPreSel) document.getElementById('evolPrescricaoId').value = prescricaoIdPreSel;
            if (cidPreSel) document.getElementById('evolCidId').value = cidPreSel;
            modalEvol.show();
            return;
        }

        // ler ou editar — buscar dados
        const resp = await fetch(`/EvolucaoClinica/Detalhar?id=${id}`).then(r => r.json());
        if (!resp.success) {
            Swal.fire('Erro', resp.message || 'Erro ao carregar evolução.', 'error');
            return;
        }
        preencherForm(resp.data);
        document.getElementById('evolucaoId').value = id;

        if (modo === 'ler') {
            tituloEl.textContent = 'Detalhes da Evolução';
            btnSalvar.style.display = 'none';
            habilitarCampos(false);
        } else {
            tituloEl.textContent = 'Editar Evolução';
            btnSalvar.style.display = '';
            habilitarCampos(true);
        }
        modalEvol.show();
    }

    async function carregarSelectsModal() {
        const [rRec, rCid] = await Promise.all([
            fetch(`/EvolucaoClinica/ListarPrescricoesDoPaciente?pacienteId=${pacienteId}`).then(r => r.json()),
            fetch(`/EvolucaoClinica/ListarCIDsDoPaciente?pacienteId=${pacienteId}`).then(r => r.json())
        ]);
        const selRec = document.getElementById('evolPrescricaoId');
        selRec.innerHTML = '<option value="">Nenhuma</option>' +
            (rRec.success ? rRec.data.map(x => {
                const d = new Date(x.dataCriacao).toLocaleDateString('pt-BR');
                return `<option value="${x.id}">${d} — ICT ${x.ict.toFixed(2)}</option>`;
            }).join('') : '');

        const selCid = document.getElementById('evolCidId');
        selCid.innerHTML = '<option value="">Nenhum</option>' +
            (rCid.success ? rCid.data.map(x => {
                const titulo = (x.titulo || '').replace(/^[\-\s]+/, '');
                return `<option value="${x.id}">${titulo}</option>`;
            }).join('') : '');
    }

    function preencherForm(d) {
        document.getElementById('evolDataConsulta').value = (d.dataConsulta || '').substring(0, 10);
        document.getElementById('evolPrescricaoId').value = d.prescricaoID ?? '';
        document.getElementById('evolCidId').value = d.categoriaCID_ID ?? '';
        document.querySelector(`input[name="evolStatus"][value="${d.status}"]`).checked = true;

        document.getElementById('evolSubjetivo').value = d.subjetivo ?? '';
        document.getElementById('evolAvaliacao').value = d.avaliacao ?? '';
        document.getElementById('evolPlano').value = d.plano ?? '';
        document.getElementById('evolObsObj').value = d.observacaoObjetivo ?? '';

        document.getElementById('evolPaSist').value = d.paSistolica ?? '';
        document.getElementById('evolPaDiast').value = d.paDiastolica ?? '';
        document.getElementById('evolFc').value = d.frequenciaCardiaca ?? '';
        document.getElementById('evolPeso').value = d.peso ?? '';
        document.getElementById('evolSpo2').value = d.spo2 ?? '';

        document.getElementById('evolGlicemia').value = d.glicemia ?? '';
        document.getElementById('evolHba1c').value = d.hba1c ?? '';
        document.getElementById('evolCreatinina').value = d.creatinina ?? '';

        document.getElementById('evolEventos').value = d.eventosAdversos ?? 0;
        document.getElementById('evolHosp').value = d.hospitalizacoes ?? 0;
        document.getElementById('evolEmerg').value = d.idasEmergencia ?? 0;
    }

    function habilitarCampos(enabled) {
        const form = document.getElementById('formEvolucao');
        form.querySelectorAll('input, select, textarea').forEach(el => {
            if (el.type !== 'hidden') el.disabled = !enabled;
        });
    }

    // Expor handlers globalmente para uso inline (botões)
    window.__evol = { carregarEvolucoes, abrirModalEvolucao };

    // Botão "Nova Evolução"
    document.getElementById('btnNovaEvolucao')?.addEventListener('click', () => {
        abrirModalEvolucao({ modo: 'criar' });
    });

    // Event delegation para botões da tabela
    document.querySelector('#tabelaEvolucoes tbody').addEventListener('click', (e) => {
        const btnVer = e.target.closest('.btn-ver-evol');
        const btnEdit = e.target.closest('.btn-editar-evol');
        const btnDel = e.target.closest('.btn-deletar-evol');
        if (btnVer) abrirModalEvolucao({ modo: 'ler', id: parseInt(btnVer.dataset.id, 10) });
        if (btnEdit) abrirModalEvolucao({ modo: 'editar', id: parseInt(btnEdit.dataset.id, 10) });
        if (btnDel) confirmarDeletar(parseInt(btnDel.dataset.id, 10));
    });

    async function confirmarDeletar(id) {
        const result = await Swal.fire({
            icon: 'warning',
            title: 'Excluir evolução?',
            text: 'Esta ação não poderá ser desfeita.',
            showCancelButton: true,
            confirmButtonText: 'Excluir',
            cancelButtonText: 'Cancelar',
            confirmButtonColor: '#dc3545'
        });
        if (!result.isConfirmed) return;

        try {
            const resp = await fetch(`/EvolucaoClinica/Deletar?id=${id}`, { method: 'POST' }).then(r => r.json());
            if (!resp.success) {
                Swal.fire('Erro', resp.message || 'Falha ao excluir.', 'error');
                return;
            }
            Swal.fire({ icon: 'success', title: 'Excluído!', timer: 1200, showConfirmButton: false });
            await carregarEvolucoes();
        } catch {
            Swal.fire('Erro', 'Falha de comunicação com o servidor.', 'error');
        }
    }

    document.getElementById('formEvolucao').addEventListener('submit', async (e) => {
        e.preventDefault();
        const id = parseInt(document.getElementById('evolucaoId').value || '0', 10);
        const payload = montarPayload();

        try {
            const url = id > 0
                ? `/EvolucaoClinica/Atualizar?id=${id}`
                : `/EvolucaoClinica/Salvar`;
            const resp = await fetch(url, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(payload)
            }).then(r => r.json());

            if (!resp.success) {
                Swal.fire('Erro', resp.message || 'Erro ao salvar.', 'error');
                return;
            }
            modalEvol.hide();
            Swal.fire({ icon: 'success', title: 'Evolução salva!', timer: 1500, showConfirmButton: false });
            await carregarEvolucoes();
        } catch {
            Swal.fire('Erro', 'Falha de comunicação com o servidor.', 'error');
        }
    });

    function montarPayload() {
        const num  = (id) => { const v = document.getElementById(id).value; return v === '' ? null : parseFloat(v); };
        const int_ = (id) => { const v = document.getElementById(id).value; return v === '' ? null : parseInt(v, 10); };
        const intN = (id) => { const v = document.getElementById(id).value; return v === '' ? 0    : parseInt(v, 10); };
        const txt  = (id) => { const v = document.getElementById(id).value.trim(); return v === '' ? null : v; };
        const sel  = (id) => { const v = document.getElementById(id).value; return v === '' ? null : parseInt(v, 10); };

        return {
            PacienteID: pacienteId,
            PrescricaoID: sel('evolPrescricaoId'),
            CategoriaCID_ID: sel('evolCidId'),
            DataConsulta: document.getElementById('evolDataConsulta').value,
            Status: parseInt(document.querySelector('input[name="evolStatus"]:checked').value, 10),

            Subjetivo: txt('evolSubjetivo'),
            Avaliacao: txt('evolAvaliacao'),
            Plano: txt('evolPlano'),
            ObservacaoObjetivo: txt('evolObsObj'),

            PaSistolica: int_('evolPaSist'),
            PaDiastolica: int_('evolPaDiast'),
            FrequenciaCardiaca: int_('evolFc'),
            Peso: num('evolPeso'),
            Spo2: int_('evolSpo2'),

            Glicemia: num('evolGlicemia'),
            Hba1c: num('evolHba1c'),
            Creatinina: num('evolCreatinina'),

            EventosAdversos: intN('evolEventos'),
            Hospitalizacoes: intN('evolHosp'),
            IdasEmergencia: intN('evolEmerg')
        };
    }

    document.getElementById('graficosDetalhados')?.addEventListener('shown.bs.collapse', () => {
        if (!detalhesRenderizados && serieCache) {
            renderizarGraficosDetalhados(serieCache);
            detalhesRenderizados = true;
        }
    });

})();
