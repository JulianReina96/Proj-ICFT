/**
* Template Name: Impact - v1.0.0
* Template URL: https://bootstrapmade.com/impact-bootstrap-business-website-template/
* Author: BootstrapMade.com
* License: https://bootstrapmade.com/license/
*/

document.addEventListener('DOMContentLoaded', () => {
  "use strict";

  /**
   * Preloader
   */
  const preloader = document.querySelector('#preloader');
  if (preloader) {
    window.addEventListener('load', () => {
      preloader.remove();
    });
  }

  /**
   * Sticky Header on Scroll
   */
  const selectHeader = document.querySelector('#header');
  if (selectHeader) {
    let headerOffset = selectHeader.offsetTop;
    let nextElement = selectHeader.nextElementSibling;

    const headerFixed = () => {
      if ((headerOffset - window.scrollY) <= 0) {
        selectHeader.classList.add('sticked');
        if (nextElement) nextElement.classList.add('sticked-header-offset');
      } else {
        selectHeader.classList.remove('sticked');
        if (nextElement) nextElement.classList.remove('sticked-header-offset');
      }
    }
    window.addEventListener('load', headerFixed);
    document.addEventListener('scroll', headerFixed);
  }

  /**
   * Navbar links active state on scroll
   */
  let navbarlinks = document.querySelectorAll('#navbar a');

  function navbarlinksActive() {
    navbarlinks.forEach(navbarlink => {

      if (!navbarlink.hash) return;

      let section = document.querySelector(navbarlink.hash);
      if (!section) return;

      let position = window.scrollY + 200;

      if (position >= section.offsetTop && position <= (section.offsetTop + section.offsetHeight)) {
        navbarlink.classList.add('active');
      } else {
        navbarlink.classList.remove('active');
      }
    })
  }
  window.addEventListener('load', navbarlinksActive);
  document.addEventListener('scroll', navbarlinksActive);

  /**
   * Mobile nav toggle
   */
  const mobileNavShow = document.querySelector('.mobile-nav-show');
  const mobileNavHide = document.querySelector('.mobile-nav-hide');

  document.querySelectorAll('.mobile-nav-toggle').forEach(el => {
    el.addEventListener('click', function(event) {
      event.preventDefault();
      mobileNavToogle();
    })
  });

  function mobileNavToogle() {
    document.querySelector('body').classList.toggle('mobile-nav-active');
    mobileNavShow.classList.toggle('d-none');
    mobileNavHide.classList.toggle('d-none');
  }

  /**
   * Hide mobile nav on same-page/hash links
   */
  document.querySelectorAll('#navbar a').forEach(navbarlink => {

    if (!navbarlink.hash) return;

    let section = document.querySelector(navbarlink.hash);
    if (!section) return;

    navbarlink.addEventListener('click', () => {
      if (document.querySelector('.mobile-nav-active')) {
        mobileNavToogle();
      }
    });

  });

  /**
   * Toggle mobile nav dropdowns
   */
  const navDropdowns = document.querySelectorAll('.navbar .dropdown > a');

  navDropdowns.forEach(el => {
    el.addEventListener('click', function(event) {
      if (document.querySelector('.mobile-nav-active')) {
        event.preventDefault();
        this.classList.toggle('active');
        this.nextElementSibling.classList.toggle('dropdown-active');

        let dropDownIndicator = this.querySelector('.dropdown-indicator');
        dropDownIndicator.classList.toggle('bi-chevron-up');
        dropDownIndicator.classList.toggle('bi-chevron-down');
      }
    })
  });

 


  /**
   * Initiate Pure Counter
   */
  new PureCounter();

  /**
   * Clients Slider
   */
  new Swiper('.clients-slider', {
    speed: 400,
    loop: true,
    autoplay: {
      delay: 5000,
      disableOnInteraction: false
    },
    slidesPerView: 'auto',
    pagination: {
      el: '.swiper-pagination',
      type: 'bullets',
      clickable: true
    },
    breakpoints: {
      320: {
        slidesPerView: 2,
        spaceBetween: 40
      },
      480: {
        slidesPerView: 3,
        spaceBetween: 60
      },
      640: {
        slidesPerView: 4,
        spaceBetween: 80
      },
      992: {
        slidesPerView: 6,
        spaceBetween: 120
      }
    }
  });

  /**
   * Init swiper slider with 1 slide at once in desktop view
   */
  new Swiper('.slides-1', {
    speed: 600,
    loop: true,
    autoplay: {
      delay: 5000,
      disableOnInteraction: false
    },
    slidesPerView: 'auto',
    pagination: {
      el: '.swiper-pagination',
      type: 'bullets',
      clickable: true
    },
    navigation: {
      nextEl: '.swiper-button-next',
      prevEl: '.swiper-button-prev',
    }
  });

  /**
   * Init swiper slider with 3 slides at once in desktop view
   */
  new Swiper('.slides-3', {
    speed: 600,
    loop: true,
    autoplay: {
      delay: 5000,
      disableOnInteraction: false
    },
    slidesPerView: 'auto',
    pagination: {
      el: '.swiper-pagination',
      type: 'bullets',
      clickable: true
    },
    navigation: {
      nextEl: '.swiper-button-next',
      prevEl: '.swiper-button-prev',
    },
    breakpoints: {
      320: {
        slidesPerView: 1,
        spaceBetween: 40
      },

      1200: {
        slidesPerView: 3,
      }
    }
  });

  /**
   * Porfolio isotope and filter
   */
  let portfolionIsotope = document.querySelector('.portfolio-isotope');

  if (portfolionIsotope) {

    let portfolioFilter = portfolionIsotope.getAttribute('data-portfolio-filter') ? portfolionIsotope.getAttribute('data-portfolio-filter') : '*';
    let portfolioLayout = portfolionIsotope.getAttribute('data-portfolio-layout') ? portfolionIsotope.getAttribute('data-portfolio-layout') : 'masonry';
    let portfolioSort = portfolionIsotope.getAttribute('data-portfolio-sort') ? portfolionIsotope.getAttribute('data-portfolio-sort') : 'original-order';

    window.addEventListener('load', () => {
      let portfolioIsotope = new Isotope(document.querySelector('.portfolio-container'), {
        itemSelector: '.portfolio-item',
        layoutMode: portfolioLayout,
        filter: portfolioFilter,
        sortBy: portfolioSort
      });

      let menuFilters = document.querySelectorAll('.portfolio-isotope .portfolio-flters li');
      menuFilters.forEach(function(el) {
        el.addEventListener('click', function() {
          document.querySelector('.portfolio-isotope .portfolio-flters .filter-active').classList.remove('filter-active');
          this.classList.add('filter-active');
          portfolioIsotope.arrange({
            filter: this.getAttribute('data-filter')
          });
          if (typeof aos_init === 'function') {
            aos_init();
          }
        }, false);
      });

    });

  }

  /**
   * Animation on scroll function and init
   */
  function aos_init() {
    AOS.init({
      duration: 1000,
      easing: 'ease-in-out',
      once: true,
      mirror: false
    });
  }
  window.addEventListener('load', () => {
    aos_init();
  });

});



//TCLE Validation



//Email Validation

function validaEmail(email) {
    var pattern = /^[^ ]+@[^ ]+\.[a-z]{2,3}$/;
    return email.match(pattern);
}
function verificarEmail() {
    var email = $("#email").val();
    return (validaEmail(email));
}

function validarEmail() {
    if (!verificarEmail()) {
        Swal.fire({
            type: 'error',
            customClass: 'swal-wide',
            title: 'E-mail inv\u00e1lido!',
            text: 'O e-mail informado \u00e9 inv\u00e1lido!',
        })
    }
}



//Phone Validation

function validarTelefone(telefone) {
    //retira todos os caracteres menos os numeros
    telefone = telefone.replace(/\D/g, '');

    //verifica se tem a qtde de numero correto
    if (!(telefone.length >= 10 && telefone.length <= 11)) return false;

    //Se tiver 11 caracteres, verificar se começa com 9 o celular
    if (telefone.length == 11 && parseInt(telefone.substring(2, 3)) != 9) return false;

    //verifica se não é nenhum numero digitado errado (propositalmente)
    for (var n = 0; n < 10; n++) {
        //um for de 0 a 9.
        //estou utilizando o metodo Array(q+1).join(n) onde "q" é a quantidade e n é o
        //caractere a ser repetido
        if (telefone == new Array(11).join(n) || telefone == new Array(12).join(n)) return false;
    }
    //DDDs validos
    var codigosDDD = [11, 12, 13, 14, 15, 16, 17, 18, 19,
        21, 22, 24, 27, 28, 31, 32, 33, 34,
        35, 37, 38, 41, 42, 43, 44, 45, 46,
        47, 48, 49, 51, 53, 54, 55, 61, 62,
        64, 63, 65, 66, 67, 68, 69, 71, 73,
        74, 75, 77, 79, 81, 82, 83, 84, 85,
        86, 87, 88, 89, 91, 92, 93, 94, 95,
        96, 97, 98, 99];
    //verifica se o DDD é valido (sim, da pra verificar rsrsrs)
    if (codigosDDD.indexOf(parseInt(telefone.substring(0, 2))) == -1) return false;

    if (new Date().getFullYear() < 2017) return true;
    if (telefone.length == 10 && [2, 3, 4, 5, 7].indexOf(parseInt(telefone.substring(2, 3))) == -1) return false;

    //se passar por todas as validações acima, então está tudo certo
    return true;
}

function verificarTelefone() {
    var telefone = $("#telefone").val();

    return (validarTelefone(telefone));
}

function validaTelefone() {
    if (!verificarTelefone()) {
        Swal.fire({
            type: 'error',
            customClass: 'swal-wide',
            title: 'Telefone inv\u00e1lido!',
            text: 'O telefone informado \u00e9 inv\u00e1lido!',
        })
    }
}

//Name Validation

function verificarNome() {
    var nome = $("#nome").val();
    return (nome.length > 10 && nome.length < 100);
}

function validarNome() {
    if (!verificarNome()) {
        Swal.fire({
            customClass: 'swal-wide',
            type: "error",
            text: "O nome deve possuir no m\u00ednimo 10 e no m\u00e1ximo 100 caracteres"
        })
    }
}

function verificaFormacao() {
    var formacao = $("#formacao").val();
    return (formacao.length < 20);
      
}

function validarFormacao() {
    if (!verificaFormacao()) {
        Swal.fire({
            type: 'error',
            customClass: 'swal-wide',      
            text: 'O campo de formaç\u00e2o deve possuir no m\u00e1ximo 20 caracteres!',
        })
    }
}

$(document).ready(function () {
    var listaMarcadores = [];

    $(".spinner").spinner({
        min: 0,
        max: 100,
        change: function (event, ui) {
            let aproximacao = parseFloat($(this).spinner('value')).toFixed(2);
            $(this).spinner('value', aproximacao);

            if ($(this).spinner('value') == null)
                $(this).spinner('value', parseFloat($(this).spinner('value'), 10) || 0);
  
        }
    });

    //valor precisa aceitar valores de 0,0 a 100,0
   




    // ------------ INICIO DE SCRIPT DE VALIDAÇÃO ---------------- //

    $('.percentualOutros').change(function () {

        //|| $(this).val() < 0)

        if ($(this).val() > 100) {

            Swal.fire({
                icon: 'error',
                customClass: 'swal-wide',
                text: 'Valor inserido está fora do intervalo permitido. Valor alterado para 0'
            })

            $(this).val(0);

            //---------- verificar erros valores negativos --------------//           
        }


    });


    //validação filhos
    $('.filho').change(function () {

        if ($(this).val() < 0) {

            Swal.fire({
                icon: 'error',
                customClass: 'swal-wide',
                text: 'N\u00e3o \u00e9 possivel inserir valores negativos.Valor alterado para 0'
            })

            $(this).val(0);

            //---------- verificar erros valores negativos --------------//           
        }

        //atribui o valor do ID do filho alterado
        var idPai = $(this).attr('data-idpai');

        
        const pai = [...document.querySelectorAll('#percentualPai[data-itemid=' + CSS.escape(idPai) + ']')];
       
        const countersFilho = [...document.querySelectorAll('#percentualFilho[data-idpai=' + CSS.escape(idPai) + ']')];
       
        //recebe todos os filhos com o mesmo ID que foi alterado
       const percentualPai = $(pai).val();
       var sumFilho=parseFloat(0);
         //const sumFilho = countersFilho.reduce((a, b) => a += parseFloat(b.value) || 10, 0);
        for (i = 0; i < countersFilho.length; i++) {
            var valorFilho = parseFloat($(countersFilho[i]).val());
            sumFilho = sumFilho + valorFilho; 
        }

        sumFilho = parseFloat(sumFilho.toFixed(2));
        //realiza a soma dos valores de todos os percentuais com ID igual ao do filho

        if (sumFilho > percentualPai) {
            Swal.fire({
                icon: 'error',
                customClass: 'swal-wide',
                html: 'A soma dos percentuais dos marcadores Sub-padr\u00f5es deve ser igual ao valor do Marcador Padr\u00e3o: <strong>'+ percentualPai + '%</strong>! Total prenchido:' + '<strong>' + sumFilho + '</strong>' + '%',
            })
            document.querySelector('#Enviarbtn').disabled = true;
            document.getElementById('percentualValidoFilhos').removeAttribute("hidden");
            for (i = 0; i < countersFilho.length; i++) {
                countersFilho[i].style.backgroundColor = "yellow";
            }

        }
        if (sumFilho < percentualPai) {
            document.getElementById('percentualValidoFilhos').removeAttribute("hidden");            
            for (i = 0; i < countersFilho.length; i++) {
                countersFilho[i].style.backgroundColor = "yellow";
              
            }
        }

        if (sumFilho == percentualPai) {

            for (i = 0; i < countersFilho.length; i++) {
                countersFilho[i].style.backgroundColor = "transparent";
            }

            const verificacao = document.querySelectorAll('.filho');
            var count = 0;

            for (i = 0; i < verificacao.length; i++) {
                if (verificacao[i].style.backgroundColor != "transparent")
                    count++;
            }

            if (count == 0) {
                document.getElementById('percentualValidoFilhos').setAttribute("hidden", "hidden");
            }
        }

        Habilitarbtn();

    });

    //validação pais
    $('.pai').change(function () {



        if ($(this).val() < 0) {

            Swal.fire({
                icon: 'error',
                customClass: 'swal-wide',
                html: 'N\u00e3o \u00e9 possivel inserir valores negativos. Valor alterado para 0'
            })

            $(this).val(0);

            //---------- verificar erros valores negativos --------------//           
        }


        var listaPercentual = [...document.querySelectorAll('#percentualPai, #percentualFilho')];



        //verifica valor do pai. Se maior que 0, habilita preenchimento dos filhos 
        var id = $(this).attr('data-itemid');
        var filhos = [...document.querySelectorAll('#percentualFilho[data-idpai=' + CSS.escape(id) + ']')];
        var valor = $(this).val();
        if (valor > 0) {
            for (i = 0; i < filhos.length; i++) {
                filhos[i].removeAttribute("hidden");
                filhos[i].value = 0;
                filhos[i].style.backgroundColor = "yellow";
                Swal.fire({
                    icon: 'warning',
                    customClass: 'swal-wide',
                    html: ' A soma dos marcadores sub-padr\u00f5es deve ser igual ao total inserido no marcador acima '
                })

            }
        }
        if (valor == 0) {
            for (i = 0; i < filhos.length; i++) {
                filhos[i].setAttribute("hidden", "hidden");
                filhos[i].value = 0;
                filhos[i].style.backgroundColor = "transparent";

            }
        }
        //fim da liberação dos filhos

        // Inicio do somatório de todos os valores pais para validação
        const countersPai = [...document.querySelectorAll('#percentualPai')];

        var sumPai = parseFloat(0);
        
        for (i = 0; i < countersPai.length; i++) {
            var valorPai = parseFloat($(countersPai[i]).val());
            sumPai = sumPai + valorPai;
        }

        sumPai = parseFloat(sumPai.toFixed(2));

       //const sumPai = countersPai.reduce((a, b) => a += parseFloat(b.value) || 0, 0);


        if (sumPai > 100) {
            Swal.fire({
                icon: 'error',
                customClass: 'swal-wide',
                html: 'A soma dos percentuais deve ser igual a <strong>100%!</strong> Total prenchido:' + '<strong>' + sumPai + '</strong>' + '%',
            })

            document.querySelector('#Enviarbtn').disabled = true;
            document.getElementById('percentualValidoPais').removeAttribute("hidden");
            for (i = 0; i < countersPai.length; i++) {
                countersPai[i].style.backgroundColor = "yellow";
            }

        }

        if (sumPai <= 100) {
            document.getElementById('percentualValidoPais').setAttribute("hidden", "hidden");
            for (i = 0; i < countersPai.length; i++) {
                countersPai[i].style.backgroundColor = "transparent";
            }

            var id = $(this).attr('data-itemid');
            var percentual = $(this).val();

            $("#Marcadores").val(listaMarcadores);
            //fim do somatório e validação dos pais

            Habilitarbtn();
            //criarLista();
            //adicionar ao submit

        }
    });

    function Habilitarbtn() {
        //recebe todos os percentuais pais e filhos e verifica se existe algum somatório invalido pela cor(yellow)
        var spinners = [...document.querySelectorAll('.pai,.filho')];
        count = 0;

            for (i = 0; i < spinners.length; i++) {
                if (spinners[i].style.backgroundColor != "transparent")
                    count++;
            }

            if (count == 0) {
                $('#Enviarbtn').prop('disabled', false);
                document.getElementById('percentualValidoFilhos').setAttribute("hidden", "hidden");
                //remove aviso dos filhos, apenas se não existir erro nos filhos
            }

            if (count != 0)
                $('#Enviarbtn').prop('disabled', true);


    }




});

function criarLista() {

    var listaMarcadores = [...document.querySelectorAll('#percentualPai, #percentualFilho')];
    var listaResultado = [];
    var count = 0;
    var sumFilho = 0;
    var totalPai = 0;
    var ValidacaoMarcadores = 0;
    
    for (i = 0; i < listaMarcadores.length; i++) { 
        var id = $(listaMarcadores[count]).attr('data-itemid');
        var percentual = $(listaMarcadores[count]).val();

        if (listaMarcadores[count].id == 'percentualFilho') {

            for (c = 0; c < listaMarcadores.length; c++) {
                if ($(listaMarcadores[c]).attr('data-itemid') == $(listaMarcadores[count]).attr('data-idpai'))
                    totalPai = $(listaMarcadores[c]).val();
                parseFloat(totalPai);
            }

            sumFilho = parseFloat(percentual) + sumFilho;

            
        }
        if (listaMarcadores[count].id == 'percentualPai') {
            if (sumFilho < totalPai) {
                event.preventDefault();
                Swal.fire({
                    icon: 'warning',
                    customClass: 'swal-wide',
                    html: ' A soma dos marcadores sub-padr\u00f5es deve ser igual ao total inserido no marcador acima '
                })//.then(function () {
            //        window.location = "" //$(listaMarcadores[count]).data('itemid'); //data-itemid(tentar usar como redirecionamento)
            //    });
            }

            else {
                ValidacaoMarcadores = ValidacaoMarcadores + parseFloat(percentual);

                    totalPai = 0;
                    sumFilho = 0;
                }
            }

        

        

      
    listaResultado.push({
        MarcadorID: id,
        Percentual: percentual

    });
        count++

    }


    
    var ListaResultado = JSON.stringify(listaResultado);

    document.getElementById('listaResultado').value = ListaResultado;

    var listaOutrosMarcadoresDescricao = [...document.querySelectorAll('#DescricaoOutroMarcador')];
    var listaOutrosMarcadoresPercentual = [...document.querySelectorAll('#PercentualOutros')];
    var listaOutrosResultados = [];
    var count = 0;
    for (i = 0; i < listaOutrosMarcadoresDescricao.length; i++) {
        var descricao = $(listaOutrosMarcadoresDescricao[count]).val();
        var percentual = $(listaOutrosMarcadoresPercentual[count]).val();
      

        listaOutrosResultados.push({
            Descricao: descricao,
            Percentual: percentual

        });
        count++

    }
    var ListaOutrosResultados = JSON.stringify(listaOutrosResultados);

    document.getElementById('listaOutrosResultados').value = ListaOutrosResultados;

    if (ValidacaoMarcadores < 100) {
        event.preventDefault();
        Swal.fire({
            icon: 'error',
            customClass: 'swal-wide',
            html: 'A soma dos percentuais preenchidos \u00e9 inferior a <strong>100%!</strong> Total preenchido:<strong>' + ValidacaoMarcadores + '%</strong>',
        })


    }

   // teste.forEach(VerificarValores)

    //validarOutrosMarcadores();
   
   
   
}
//fim da validação

function validarFormulario() {
    return false;
}

//gerar campos outros marcadores



function criarCampoOutrosMarcadores() {

    var outrosMarcadores = [...document.querySelectorAll('#DescricaoOutroMarcador')];


    var IdAtribuido = Math.random();

    var i = 0;

    while ( i <= outrosMarcadores.count) {

        if (outrosMarcadores.id != IdAtribuido)
            i++
        if (outrosMarcadores.id == IdAtribuido) {
            IdAtribuido = Math.random();
            i = 0;
        }


    }

    //usar contagem para definir ids unicos

    var Id = IdAtribuido.toString();

    var LinhaColuna = document.createElement('tr');
    LinhaColuna.id = Id;
    LinhaColuna.className = 'OutrosMarcadores'
    
    var IncluirMarcador = document.createElement('input');
    IncluirMarcador.type = "text";
    IncluirMarcador.required = 'true';
    //excluir.value = valor.substring(12);
    //excluir.style.cursor = 'pointer'

    IncluirMarcador.id = 'DescricaoOutroMarcador'

    //var IncluirCheckbox = document.createElement('input');
    //IncluirCheckbox.type = "checkbox";
    //IncluirCheckbox.style.cursor = 'pointer'
   

    var TdNomeMarcador = document.createElement('td');
    //fileName.id = valor.substring(12)
    //fileName.innerText = valor.substring(12);
    TdNomeMarcador.className = 'OutroMarcador MarcadorSize'
   
    //fileName.append(IncluirPercentual)
    var DivNomeMarcador = document.createElement('div');
    DivNomeMarcador.style = 'margin-left:10px; width:500px'
    DivNomeMarcador.className = 'MarcadorSize'
    DivNomeMarcador.append(IncluirMarcador)
    TdNomeMarcador.append(DivNomeMarcador)

    LinhaColuna.append(TdNomeMarcador)
    //inclusão de campo para inserção de nome outro marcador

    //$("#NovoMarcador").append(LinhaColuna)

    var IncluirPercentual = document.createElement('input');
    IncluirPercentual.type = "number"
    IncluirPercentual.step = "0.01"
    IncluirPercentual.className = "form-control spinner ui-spinner percentualOutros"
    IncluirPercentual.style = "width:100px"
    IncluirPercentual.id = 'PercentualOutros'
    IncluirPercentual.min = 0;
    IncluirPercentual.max = 100;
    IncluirPercentual.required = 'true';
   



    var TdPercentual = document.createElement('td');
   
   
    

    var DivPercentualMarcador = document.createElement('div');
   
    DivPercentualMarcador.className = 'inputPercentual col-1'
    DivPercentualMarcador.align = 'center'
    DivPercentualMarcador.append(IncluirPercentual)
    TdPercentual.append(DivPercentualMarcador)

    LinhaColuna.append(TdPercentual)

    var btnExcluir = document.createElement('button');
    btnExcluir.onclick = excluirOutro;
    btnExcluir.id = Id;
    btnExcluir.type = 'button';
    btnExcluir.className = 'btn btn-danger btn-md col-sm'
    btnExcluir.innerHTML = 'X';
    btnExcluir.style = 'margin-top:25px'
    LinhaColuna.append(btnExcluir)


    $("#NovoMarcador").append(LinhaColuna)

}

function excluirOutro(event) {

    var remover = event.target.id;
    var OutrosMarcadores = [...document.querySelectorAll('.OutrosMarcadores')];
    var count = 0;
    for (i = 0; i <= OutrosMarcadores.length -1; i++) {
        if (remover == OutrosMarcadores[i].id) {
            OutrosMarcadores[i].remove();
            //OutrosMarcadores[i].id = count;
            count++
            
        }
    }

}

function validarPesquisador() {

    const pesquisador = documento.getElementById('EscolhaPesquisadores');
    const pesquisadores = [...document.querySelectorAll('.todosPesquisadores')];
    var Ok = 0;

    for (i = 0; i <= pesquisadores.length; i++) {
        if (pesquisadores[i].value() == pesquisador.value())
            Ok = 1;
        i++;
    }
    if (Ok == 0) {

        Swal.fire({
            icon: 'error',
            customClass: 'swal-wide',
            html: 'Pesquisador não encontrado no sistema. Insira um pesquisador válido'
        })
        return false;
    }
    else
        return true;


 }


//function VerificarValores(item) {

//    if ($(item).val() > 100 || $(item).val() < 0) {
        
//        document.querySelector('#Enviarbtn').disabled = true;
//        document.getElementById('percentualValidoOutros').removeAttribute("hidden");
//        item.style.backgroundColor = "yellow";
        
//    }
//    else {
//        item.style.backgroundColor = "transparent";
       
//    }

//}

   


//function validarOutrosMarcadores() {

//    var teste = [...document.querySelectorAll('#PercentualOutros')];
//    var naoValidado;
//    for (i = 1; i <= teste.length; i++) {
//        if (teste[i].style.backgroundColor == "yellow")
//            naoValidado = 1;
//    }
//    if (naoValidado != 1) {
//        document.querySelector('#Enviarbtn').disabled = false;
//        document.getElementById('percentualValidoOutros').setAttribute("hidden", "hidden");
//    }
//}


//document.getElementById("Cadastrar").onclick = function () {
//    if (!validarCampos()) {
//        Swal.fire({
//            type: 'error',
//            customClass: 'swal-wide',
//            title: 'Formulário incompleto!',
//            text: 'Preencha todos os campos corretamente.',
//        })
//    }
//};

// onsubmit="return verificarCampos();"


//firebase with google//

