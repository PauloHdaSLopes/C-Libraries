using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Paulo.Console.model
{
    public class Pedido
    {
        public string pedidoMarsh { get; set; }
        public string segurado { get; set; }
        public string cpfCnpj { get; set; }
        public string categoria { get; set; }
        public string modalidade { get; set; }
        public string vigencia { get; set; }
        public string Iniciovigencia { get; set; }
        public string Fimvigencia { get; set; }
        public string premio { get; set; }
        public string premioTotal { get; set; }
        public string tipoDocumento { get; set; }

        //public string moeda { get; set; }
        //public string dataEmissao { get; set; }
        //public string participacao { get; set; }
        //public string valor { get; set; }
        //public string enderecoCobranca { get; set; }
        //public string enderecoCorrespondencia { get; set; }
        //public List<Segurado> segurados { get; set; }
        //public string companhiaSeguradora { get; set; }
        //public string Participacao { get; set; }
        //public string enderecoSeguradora { get; set; }
        //public string adicional { get; set; }
        //public string custo { get; set; }
        //public string iof { get; set; }
        //public string corretora2 { get; set; }
        //public string distribuicao { get; set; }
        //public string codigoSusep { get; set; }
        //public string accountResponsavel { get; set; }
        //public string importanciaSeguradoraTotal { get; set; }
        //public string percentualTaxa { get; set; }
        //public string descAgravo { get; set; }
        //public string franquiaPos { get; set; }
        //public string clausulaDdr { get; set; }
        //public string clausulaRevisao { get; set; }
        //public string premioMinimo { get; set; }

        public Pedido(string texto)
        {
  
            #region PAGINA 1

            #region CABECALHO

            this.categoria = Regex.Match(texto, ".*(?= Data)").ToString();
            //this.dataEmissao = Regex.Match(texto[0], "(?<= Data de Emissão ).*").ToString();
            this.pedidoMarsh = Regex.Match(texto, "(?<=Marsh\n).+").ToString();
            if (!Regex.IsMatch(this.pedidoMarsh, "^\\d+$"))
                this.pedidoMarsh = Regex.Match(texto, @"\d{1,}(?=\nPedido Marsh)").ToString();
            this.tipoDocumento = Regex.Match(texto, "(?<=Emissão de ).+(?= Página)").ToString();

            #endregion

            #region PROPONENTE

            ////  Extrair texto da area do proponente
            var proponenteText = Regex.Match(texto, "(?=Proponente)((.|\n)*)(?=Seguradoras)").ToString();
            this.segurado = Regex.Match(proponenteText, @"(?<=Contato\n).+?(?=\d)").ToString();
            //ToDo: Ponto de atenção :: validar formatos que estão vindo o CNPJ
            this.cpfCnpj = Regex.Match(proponenteText, @"\d{15}").ToString();
            this.cpfCnpj = FormatCnpj(cpfCnpj);

            ////var segurado = Regex.Match(texto[10], @"(?<=Contato[\n\r]).+?(?= \d)").ToString();
            //this.participacao = Regex.Match(proponenteText, @"[\d]{1,3},[\d]{2}").ToString();
            //this.valor = Regex.Matches(proponenteText, @"(\d{0,3}.\d{1,3},\d{2})")[1].ToString();

            ////ToDo: Analisar lógica para capturar o dados individualmente
            //this.enderecoCobranca = Regex.Match(proponenteText, "(?<=CEP\n).+").ToString();

            ////ToDo: Analisar lógica para capturar o dados individualmente
            //this.enderecoCorrespondencia = Regex.Match(proponenteText, @"(?<=e-mail\n).+(?<=[a-zA-Z]\s)").ToString();

            ////Cria lista com a quebra de linhas para contar o numero de segurados que o pedido possui
            //var list = proponenteText.Split('\n');

            //int qtdLinhasSegurado = 0;
            //List<Segurado> segurados = new List<Segurado>();

            //while (list[9 + (qtdLinhasSegurado * 3)].StartsWith("Segurado"))
            //{
            //    var tmp = list[10 + (qtdLinhasSegurado * 3)].ToString();

            //    var segurado = Regex.Match(tmp, @".+?(?=\d)").ToString();
            //    tmp = tmp.Replace(segurado, "");

            //    
            //    tmp = tmp.Replace(cpfCnpj, "");

            //    var valores= tmp.Split('\n')[0];

            //    var participacao = valores[0].ToString();

            //    var valor = Regex.Matches(tmp, @"(\d{0,3}.\d{1,3},\d{2})")[1].ToString();

            //    segurados.Add(new Segurado(segurado, cpfCnpj, participacao, valor));

            //    qtdLinhasSegurado++;
            //}
            //this.segurados = segurados;
            #endregion

            #region SEGURADORAS

            //  Extrair texto da area das Seguradas
            //var seguradorasText = Regex.Match(texto[0], @"(?=Seguradoras)((.|\n)*)(?=Vigência \/ Prêmios)").ToString();

            //this.companhiaSeguradora = Regex.Match(seguradorasText, ".*(?= Lider)").ToString();
            //this.Participacao = Regex.Match(seguradorasText, @"(\d{1,3},\d{2})").ToString();

            //seguradorasText = Regex.Replace(seguradorasText, @"(\d{1,3},\d{2})", "");
            //this.enderecoSeguradora = Regex.Match(seguradorasText, @"(?<= Lider)(.|\n)*").ToString().Replace("\n","").Trim();

            #endregion

            #region VIGENCIA PREMIO

            //  Extrair texto da area de vigencia e premios
            var vigenciaText = Regex.Match(texto, @"(?<=Vigência \/ Prêmios\n)((.|\n)*)(?=Corretoras)").ToString();
            this.vigencia = Regex.Match(vigenciaText, @"\d{2}\/\d{2}\/\d{4} até \d{2}\/\d{2}\/\d{4}").ToString();
            this.Iniciovigencia = Regex.Split(vigencia, " até ")[0];
            this.Fimvigencia = Regex.Split(vigencia, " até ")[1];

            var matchs = Regex.Matches(vigenciaText, @"(\d{0,3}.\d{1,3},\d{2})");
            this.premio = matchs[0].ToString();
            //this.adicional = matchs[1].ToString();
            //this.custo = matchs[2].ToString();
            //this.iof = matchs[3].ToString();
            this.premioTotal = matchs[4].ToString();

            #endregion

            #region CORRETORAS

            //var corretoraText = Regex.Match(texto[0], "(?<=Corretoras\n)((.|\n)*)(?=Na)").ToString().Split('\n')[2];

            //this.corretora2 = Regex.Split(corretoraText, " Lider ")[0];
            //this.distribuicao = Regex.Split(corretoraText, " Lider ")[1];

            #endregion

            #region RODAPE
            //this.codigoSusep = Regex.Match(texto[0], "(?<=Código SUSEP ).+").ToString();
            //this.accountResponsavel = Regex.Match(texto[0], "(?<=Account Responsável: ).+").ToString();
            #endregion

            #endregion

            #region PAGINA 2
            //Extrair as informações do pdf em string da Segunda Página
            this.modalidade = Regex.Match(texto, "(?<=Modalidade ).+(?= Cobertura)").ToString();
            //this.importanciaSeguradoraTotal = Regex.Match(texto[1], @"(?<=Importância Segurada Total ).+?(?=\s)").ToString();
            //this.moeda = Regex.Match(texto[1], @"(?<=Moeda ).+").ToString();
            //this.percentualTaxa = Regex.Match(texto[1], @"(?<=% Taxa ).+(?= %)").ToString();
            //this.descAgravo = Regex.Match(texto[1], @"(?<=Desc\/Agravo ).+").ToString();
            //this.franquiaPos = Regex.Match(texto[1], @"(?<=Franquia \/ POS Dedutível)(.|\n)+(?=Cláusula DDR)").ToString();
            //var infoTmp = Regex.Match(texto[1], @"(?<=Conta Mensal\n).*").ToString();
            //this.clausulaDdr = Regex.Match(infoTmp, @"[a-zA-Z]{1,}").ToString();
            //this.clausulaRevisao = Regex.Match(infoTmp, @"(?<=Importância Segurada Total ).+?(?=\s)").ToString();
            //this.premioMinimo = Regex.Match(infoTmp, @"(?<=Importância Segurada Total ).+?(?=\s)").ToString();
            #endregion
        }

        private string FormatCnpj(string text)
        {
            if (text[0] == '0' && text.Length > 14)
                return string.Format("{0}{1}.{2}{3}{4}.{5}{6}{7}/{8}{9}{10}{11}-{12}{13}"
                , text[1], text[2], text[3], text[4], text[5], text[6], text[7], text[8], text[9], text[10], text[11], text[12], text[13], text[14]);
            
            return string.Format("{0}{1}.{2}{3}{4}.{5}{6}{7}/{8}{9}{10}{11}-{12}{13}"
                            , text[0], text[1], text[2], text[3], text[4], text[5], text[6], text[7], text[8], text[9], text[10], text[11], text[12], text[13]);
        }
                
        public override string ToString()
        {
            string text = "";
            foreach (var prop in this.GetType().GetProperties())
            {
                text += prop.Name + " : " + prop.GetValue(this, null) + "\n";
            }
            return text;
        }
    }

}