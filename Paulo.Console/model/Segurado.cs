namespace Paulo.Console.model
{
    public class Segurado
    {
        string _segurado        { get; set; }
        string _cpfCnpj         { get; set; }
        string _participacao    { get; set; }
        string _valor           { get; set; }

        public Segurado(string _segurado,string _cpfCnpj,string _participacao,string _valor)
        {
           this._segurado = _segurado;
           this._cpfCnpj = _cpfCnpj;
           this._participacao = _participacao;
           this._valor = _valor;
        }

        public void ToString()
        {
            System.Console.WriteLine("Dados do pedido:");

            foreach (var prop in this.GetType().GetProperties())
            {
                System.Console.WriteLine(prop.Name + " : " + prop.GetValue(this, null));
            }

        }

    }
}
