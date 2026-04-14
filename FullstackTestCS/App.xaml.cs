using DbUp;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using System.Configuration;
using System.Data;
using System.Reflection;
using System.Windows;

namespace FullstackTestCS
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            ExecutarMigracoesBanco();
        }

        private void ExecutarMigracoesBanco()
        {
            try
            {
                var config = new ConfigurationBuilder()
                                    .AddUserSecrets<App>()
                                    .Build();
                var builder = new MySqlConnectionStringBuilder
                {
                    Server = "localhost",
                    Port = uint.Parse(config["DbPort"]),
                    Database = config["DbName"],
                    UserID = config["DbUser"],
                    Password = config["DbPassword"]
                };

                EnsureDatabase.For.MySqlDatabase(builder.ConnectionString);
                var upgrader = DeployChanges.To
                    .MySqlDatabase(builder.ConnectionString)
                    .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly())
                    .LogToConsole()
                    .Build();

                var result = upgrader.PerformUpgrade();

                if (!result.Successful)
                {
                    MessageBox.Show("Erro na migração: " + result.Error);
                    Shutdown();
                }
            }
            catch (FormatException e)
            {
                MessageBox.Show("Erro de conversao:\n" + e.Message);

            }
            catch (ArgumentNullException e)
            {
                MessageBox.Show("Erro de conversao(variavel nula):\n" + e.Message);

            }
            catch (MySqlException e)
            {
                MessageBox.Show("Erro ao conectar ao banco:\n" + e.Message);
            }
        }

        public static void InsertOperation(MySqlConnection connectionOK, string table, string nome, string idade)
        {
            try
            {
                connectionOK.Open();
                Console.WriteLine("\n> Conexao estabelecida com sucesso!");

                string sql = $"Insert Into {table}(nome, idade)Values(@nome, @idade);";
                MySqlCommand cmd = new MySqlCommand(sql, connectionOK);
                cmd.Parameters.AddWithValue("@nome", nome);
                cmd.Parameters.AddWithValue("@idade", int.Parse(idade));
                cmd.ExecuteNonQuery();

                Console.WriteLine("> Insert realizado com sucesso!");
            }
            catch (FormatException e)
            {
                System.Console.WriteLine("Erro de conversao:\n" + e.Message);

            }
            catch (ArgumentNullException e)
            {
                System.Console.WriteLine("Erro de conversao(variavel nula):\n" + e.Message);

            }
            catch (MySqlException e)
            {
                Console.WriteLine("Erro ao realizar Insert no Banco de dados:\n" + e.Message);
            }
            connectionOK.Close();
        }

        public static void UpdateOperation(MySqlConnection connectionOK, string id, string table, string nome, string idade)
        {
            try
            {
                connectionOK.Open();
                Console.WriteLine("\n> Conexão estabelecida com sucesso!");

                string sql = $"Update {table} Set nome=@nome, idade=@idade Where id=@id;";
                MySqlCommand cmd = new MySqlCommand(sql, connectionOK);
                cmd.Parameters.AddWithValue("@nome", nome);
                cmd.Parameters.AddWithValue("@idade", int.Parse(idade));
                cmd.Parameters.AddWithValue("@id", int.Parse(id));
                cmd.ExecuteNonQuery();

                Console.WriteLine("> Update realizado com sucesso!");
            }
            catch (FormatException e)
            {
                System.Console.WriteLine("Erro de conversao:\n" + e.Message);

            }
            catch (ArgumentNullException e)
            {
                System.Console.WriteLine("Erro de conversao(variavel nula):\n" + e.Message);

            }
            catch (MySqlException e)
            {
                Console.WriteLine("Erro ao realizar Update no Banco de dados:\n" + e.Message);
            }
            connectionOK.Close();
        }

        public static void DeleteOperation(MySqlConnection connectionOK, string id, string table)
        {
            try
            {
                connectionOK.Open();
                Console.WriteLine("\n> Conexão estabelecida com sucesso!");

                string sql = $"Delete From {table} Where id=@id;";
                MySqlCommand cmd = new MySqlCommand(sql, connectionOK);
                cmd.Parameters.AddWithValue("@id", int.Parse(id));
                cmd.ExecuteNonQuery();

                Console.WriteLine("> Delete realizado com sucesso!");
            }
            catch (FormatException e)
            {
                System.Console.WriteLine("Erro de conversao:\n" + e.Message);

            }
            catch (ArgumentNullException e)
            {
                System.Console.WriteLine("Erro de conversao(variavel nula):\n" + e.Message);

            }
            catch (MySqlException e)
            {
                Console.WriteLine("Erro ao realizar Delete no Banco de dados:\n" + e.Message);
            }
            connectionOK.Close();
        }

        public static void SelectOperation(MySqlConnection connectionOK, string table)
        {
            try
            {
                connectionOK.Open();
                Console.WriteLine("\n> Conexão estabelecida com sucesso!");

                string sql = $"SELECT * From {table}";
                MySqlCommand cmd = new MySqlCommand(sql, connectionOK);
                using (MySqlDataReader rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        Console.WriteLine($"{rdr["id"]} - {rdr["nome"]}");
                    }
                }

                Console.WriteLine("> Select realizado com sucesso!");
            }
            catch (FormatException e)
            {
                Console.WriteLine("Erro de conversao:\n" + e.Message);

            }
            catch (ArgumentNullException e)
            {
                Console.WriteLine("Erro de conversao(variavel nula):\n" + e.Message);

            }
            catch (MySqlException e)
            {
                Console.WriteLine("Erro ao realizar Select no Banco de dados:\n" + e.Message);
            }
            connectionOK.Close();
        }
    }
}
