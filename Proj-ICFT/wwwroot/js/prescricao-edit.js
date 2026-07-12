/*
 * Edição de prescrição — modal isolado (VerPaciente).
 * Reutiliza os endpoints de medicamentos/CID e os dados de referência injetados
 * em window.__prescEditRef. Não interfere no formulário de criação (Form.cshtml).
 * Expõe window.__prescricaoEdit.abrir(prescricaoId).
 */
(function () {
    'use strict';

    const ref = window.__prescEditRef;
    if (!ref || typeof $ === 'undefined') return;

    const $modal = $('#modalEditarPrescricao');
    if ($modal.length === 0) return;
    const modal = new bootstrap.Modal($modal[0]);

    let meds = [];
    let cids = [];
    let counter = 0;

    // ───────── Popular selects estáticos a partir dos dados de referência ─────────
    function popularEstaticos() {
        const $cat = $('#edit-categorias');
        $cat.find('option:not(:first)').remove();
        ref.categorias.forEach(c => $cat.append(new Option(c.name, c.id)));

        const $freq = $('#edit-frequencia');
        $freq.find('option:not(:first)').remove();
        ref.frequencias.forEach(f => {
            const o = new Option(f.name + ' — Peso(' + f.peso + ')', f.id);
            o.dataset.peso = f.peso;
            $freq.append(o);
        });

        const box = document.getElementById('edit-instrucoes-box');
        box.innerHTML = ref.instrucoes.map(i =>
            `<div class="form-check">
                <input class="form-check-input" type="checkbox" id="edit-instr-${i.id}" value="${i.id}" data-peso="${i.peso}">
                <label class="form-check-label" for="edit-instr-${i.id}">${i.name} — Peso(${i.peso})</label>
             </div>`).join('');

        const $bloco = $('#edit-blocoCid');
        $bloco.find('option:not(:first)').remove();
        ref.blocos.forEach(b => $bloco.append(new Option(b.title, b.blockId)));
    }

    // ───────── Select2 (com dropdownParent no modal) ─────────
    function initSelect2() {
        $('#edit-remedio').select2({
            placeholder: 'Buscar medicamento...', allowClear: true, width: '100%',
            dropdownParent: $modal, minimumInputLength: 2,
            language: { inputTooShort: () => 'Digite ao menos 2 caracteres' },
            ajax: {
                url: '/Medicamentos/BuscarMedicamentos', dataType: 'json', delay: 300, cache: false,
                data: params => ({ q: params.term || '', page: params.page || 1, pageSize: 20 }),
                processResults: data => (!data || !data.results) ? { results: [] }
                    : { results: data.results, pagination: { more: data.pagination && data.pagination.more } },
                error: () => ({ results: [] })
            }
        });
        $('#edit-blocoCid').select2({ placeholder: 'Selecione um grupo', allowClear: true, width: '100%', dropdownParent: $modal });
        $('#edit-enfermidadeId').select2({ placeholder: 'Primeiro selecione um grupo', allowClear: true, width: '100%', dropdownParent: $modal });
    }

    // ───────── Categoria → Tipos ─────────
    $('#edit-categorias').on('change', function () {
        const $sub = $('#edit-subcategorias').empty();
        const lista = ref.tiposPorCategoria[$(this).val()];
        if (!lista) { $sub.append(new Option('Selecione o tipo', '')); return; }
        $sub.append(new Option('Selecione o tipo', ''));
        lista.forEach(t => {
            const o = new Option(t.name + ' — Peso(' + t.peso + ')', t.id);
            o.dataset.peso = t.peso;
            $sub.append(o);
        });
    });

    // ───────── Bloco → Enfermidades (AJAX) ─────────
    $('#edit-blocoCid').on('change', function () {
        const blocoId = $(this).val();
        const $enf = $('#edit-enfermidadeId');
        $enf.select2('destroy');
        $enf.prop('disabled', true).empty().append(new Option('Primeiro selecione um grupo', ''));
        $enf.select2({ placeholder: 'Primeiro selecione um grupo', allowClear: true, width: '100%', dropdownParent: $modal });
        if (!blocoId) return;

        $.get('/CID/ListarCategoriasPorBloco', { blocoId: blocoId }, function (data) {
            $enf.select2('destroy');
            $enf.empty();
            if (!data || data.length === 0) {
                $enf.append(new Option('Nenhuma enfermidade encontrada', ''));
            } else {
                $enf.append(new Option('Selecione uma enfermidade', ''));
                data.forEach(item => {
                    const o = new Option(item.title, item.id);
                    o.dataset.code = item.code || '';
                    $enf.append(o);
                });
                $enf.prop('disabled', false);
            }
            $enf.select2({ placeholder: 'Selecione uma enfermidade', allowClear: true, width: '100%', dropdownParent: $modal });
        });
    });

    // ───────── Medicamentos ─────────
    $('#edit-addMedicamento').on('click', function () {
        const remedioId = $('#edit-remedio').val();
        const catId     = $('#edit-categorias').val();
        const subId     = $('#edit-subcategorias').val();
        const freqId    = $('#edit-frequencia').val();

        let erros = '';
        if (!remedioId) erros += '<p>Selecione um medicamento.</p>';
        if (!catId)     erros += '<p>Selecione a forma de dosagem.</p>';
        if (!subId)     erros += '<p>Selecione o tipo.</p>';
        if (!freqId)    erros += '<p>Selecione a frequência.</p>';
        if (erros) { Swal.fire({ icon: 'error', title: 'Campos obrigatórios', html: erros }); return; }

        const subOpt  = $('#edit-subcategorias option:selected')[0];
        const freqOpt = $('#edit-frequencia option:selected')[0];
        const instr = $('#edit-instrucoes-box input:checked').map(function () {
            return { id: parseInt(this.value, 10), nome: this.nextElementSibling.textContent.split(' — Peso')[0], peso: parseFloat(this.dataset.peso) || 0 };
        }).get();

        counter++;
        meds.push({
            clientId: counter,
            remedioId: parseInt(remedioId, 10),
            remedioNome: $('#edit-remedio option:selected').text().trim(),
            categoriaId: parseInt(catId, 10),
            categoriaNome: $('#edit-categorias option:selected').text().trim(),
            subId: parseInt(subId, 10),
            subNome: subOpt.text.split(' — Peso')[0],
            subPeso: parseFloat(subOpt.dataset.peso) || 0,
            freqId: parseInt(freqId, 10),
            freqNome: freqOpt.text.split(' — Peso')[0],
            freqPeso: parseFloat(freqOpt.dataset.peso) || 0,
            instr: instr
        });

        // reset controles
        $('#edit-remedio').val(null).trigger('change');
        $('#edit-categorias').prop('selectedIndex', 0);
        $('#edit-subcategorias').empty().append(new Option('Selecione o tipo', ''));
        $('#edit-frequencia').prop('selectedIndex', 0);
        $('#edit-instrucoes-box input:checked').prop('checked', false);

        renderMeds();
    });

    window.__epRemoverMed = function (clientId) {
        meds = meds.filter(m => m.clientId !== clientId);
        renderMeds();
    };

    function renderMeds() {
        const wrap = document.getElementById('edit-cards-remedios');
        document.getElementById('edit-empty-remedios').style.display = meds.length ? 'none' : '';
        wrap.innerHTML = meds.map(m => {
            const chips = `<span class="ep-chip">Forma +${m.subPeso.toFixed(1)}</span>
                           <span class="ep-chip">Freq +${m.freqPeso.toFixed(1)}</span>` +
                          (m.instr.length ? `<span class="ep-chip">Instr +${m.instr.reduce((a, i) => a + i.peso, 0).toFixed(1)}</span>` : '');
            const instrTxt = m.instr.length ? `<div class="ep-card-sub">Instruções: ${m.instr.map(i => i.nome).join(', ')}</div>` : '';
            return `<div class="ep-card">
                        <button class="ep-remove" title="Remover" onclick="__epRemoverMed(${m.clientId})"><i class="bi bi-trash"></i></button>
                        <div class="ep-card-title">${m.remedioNome}</div>
                        <div class="ep-card-sub">${m.categoriaNome} · ${m.subNome} · ${m.freqNome}</div>
                        ${instrTxt}
                        <div>${chips}</div>
                    </div>`;
        }).join('');
        atualizarICT();
    }

    // ───────── Enfermidades ─────────
    $('#edit-addEnfermidade').on('click', function () {
        const id = $('#edit-enfermidadeId').val();
        if (!id) { Swal.fire({ icon: 'warning', title: 'Atenção', text: 'Selecione uma enfermidade.' }); return; }
        if (cids.some(c => c.id === parseInt(id, 10))) { Swal.fire({ icon: 'info', title: 'Já adicionada', text: 'Esta enfermidade já está na lista.' }); return; }

        const opt = $('#edit-enfermidadeId option:selected')[0];
        counter++;
        cids.push({ clientId: counter, id: parseInt(id, 10), code: opt.dataset.code || '', title: opt.text.trim() });
        $('#edit-blocoCid').val(null).trigger('change');
        renderCids();
    });

    window.__epRemoverCid = function (clientId) {
        cids = cids.filter(c => c.clientId !== clientId);
        renderCids();
    };

    function renderCids() {
        const wrap = document.getElementById('edit-cards-enfermidades');
        document.getElementById('edit-empty-enfermidades').style.display = cids.length ? 'none' : '';
        wrap.innerHTML = cids.map(c => `
            <div class="ep-card">
                <button class="ep-remove" title="Remover" onclick="__epRemoverCid(${c.clientId})"><i class="bi bi-trash"></i></button>
                <div class="ep-card-title">${c.title}</div>
                ${c.code ? `<div class="ep-card-sub">${c.code}</div>` : ''}
            </div>`).join('');
    }

    // ───────── Estimativa de ICT (MRCI) ─────────
    function atualizarICT() {
        const box = document.getElementById('edit-ict-box');
        if (!meds.length) { box.hidden = true; return; }
        const subsDistintas = [...new Set(meds.map(m => m.subId))];
        const formas = subsDistintas.reduce((acc, id) => {
            const m = meds.find(x => x.subId === id);
            return acc + (m ? m.subPeso : 0);
        }, 0);
        const freq  = meds.reduce((a, m) => a + m.freqPeso, 0);
        const instr = meds.reduce((a, m) => a + m.instr.reduce((s, i) => s + i.peso, 0), 0);
        document.getElementById('edit-ict-formas').textContent = formas.toFixed(1);
        document.getElementById('edit-ict-freq').textContent   = freq.toFixed(1);
        document.getElementById('edit-ict-instr').textContent  = instr.toFixed(1);
        document.getElementById('edit-ict-total').textContent  = (formas + freq + instr).toFixed(2);
        box.hidden = false;
    }

    // ───────── Abrir modal com dados existentes ─────────
    async function abrir(prescricaoId) {
        // reset
        meds = []; cids = []; counter = 0;
        $('#edit-prescricaoId').val(prescricaoId);
        $('#edit-presc-id-label').text('#' + prescricaoId);
        $('#edit-remedio').val(null).trigger('change');
        $('#edit-categorias').prop('selectedIndex', 0);
        $('#edit-subcategorias').empty().append(new Option('Selecione o tipo', ''));
        $('#edit-frequencia').prop('selectedIndex', 0);
        $('#edit-instrucoes-box input:checked').prop('checked', false);
        $('#edit-blocoCid').val(null).trigger('change');
        renderMeds(); renderCids();

        try {
            const data = await fetch('/Paciente/ObterPrescricaoParaEdicao?prescricaoId=' + prescricaoId).then(r => r.json());
            if (!data.success) { Swal.fire('Erro', data.message || 'Falha ao carregar prescrição.', 'error'); return; }

            $('#edit-adesao').prop('checked', !!data.adesao);

            data.medicamentos.forEach(m => {
                counter++;
                meds.push({
                    clientId: counter,
                    remedioId: m.medicamentoId, remedioNome: m.medicamentoNome,
                    categoriaId: m.categoriaId, categoriaNome: m.categoriaNome,
                    subId: m.subcategoriaId, subNome: m.subcategoriaNome, subPeso: m.subcategoriaPeso || 0,
                    freqId: m.frequenciaId, freqNome: m.frequenciaNome, freqPeso: m.frequenciaPeso || 0,
                    instr: (m.instrucoes || []).map(i => ({ id: i.id, nome: i.nome, peso: i.peso || 0 }))
                });
            });
            (data.cids || []).forEach(c => {
                counter++;
                cids.push({ clientId: counter, id: c.id, code: c.code || '', title: c.title || '' });
            });

            renderMeds(); renderCids();
            modal.show();
        } catch {
            Swal.fire('Erro', 'Falha de comunicação com o servidor.', 'error');
        }
    }

    // ───────── Salvar alterações ─────────
    $('#edit-salvar').on('click', async function () {
        if (!meds.length) { Swal.fire({ icon: 'warning', title: 'Atenção', text: 'Adicione ao menos um medicamento.' }); return; }
        const prescricaoId = parseInt($('#edit-prescricaoId').val(), 10);

        const payload = {
            pacienteId: ref.pacienteId,
            pacienteAnonimo: false,
            adesao: $('#edit-adesao').is(':checked'),
            medicamentos: meds.map(m => ({
                medicamentoId: m.remedioId,
                categoriaId: m.categoriaId,
                subcategoriaId: m.subId,
                frequenciaId: m.freqId,
                instrucoesAdicionais: m.instr.map(i => i.id)
            })),
            cidCategorias: cids.map(c => c.id)
        };

        const btn = this;
        btn.disabled = true;
        btn.innerHTML = '<span class="spinner-border spinner-border-sm me-1"></span> Salvando...';

        try {
            const data = await fetch('/Home/AtualizarPrescricao?prescricaoId=' + prescricaoId, {
                method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(payload)
            }).then(r => r.json());

            if (data.success) {
                modal.hide();
                await Swal.fire({ icon: 'success', title: 'Prescrição atualizada!',
                    html: '<p>Novo ICT: <strong>' + data.ict.toFixed(2) + '</strong></p>',
                    timer: 1600, showConfirmButton: false });
                location.reload();
            } else {
                Swal.fire('Erro', data.message || 'Falha ao atualizar.', 'error');
                btn.disabled = false;
                btn.innerHTML = '<i class="bi bi-check2-circle"></i> Salvar alterações';
            }
        } catch {
            Swal.fire('Erro', 'Falha de comunicação com o servidor.', 'error');
            btn.disabled = false;
            btn.innerHTML = '<i class="bi bi-check2-circle"></i> Salvar alterações';
        }
    });

    // init
    popularEstaticos();
    initSelect2();

    window.__prescricaoEdit = { abrir };
})();
