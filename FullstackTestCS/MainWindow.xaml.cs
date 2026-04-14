using Microsoft.Extensions.Configuration;
using MySqlConnector;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace FullstackTestCS
{
    public partial class MainWindow : Window
    {
        private string _connectionString = string.Empty;
        private const string Table = "Alunos";

        public MainWindow()
        {
            InitializeComponent();
            InicializarConexao();
            CarregarAlunos();
        }

        // Iniciar conexao mysql
        private void InicializarConexao()
        {
            try
            {
                var config = new ConfigurationBuilder()
                    .AddUserSecrets<App>()
                    .Build();

                var builder = new MySqlConnectionStringBuilder
                {
                    Server = "localhost",
                    Port = uint.Parse(config["DbPort"]!),
                    Database = config["DbName"],
                    UserID = config["DbUser"],
                    Password = config["DbPassword"]
                };

                _connectionString = builder.ConnectionString;
                MostrarStatus(true);
            }
            catch (Exception ex)
            {
                MostrarStatus(false);
                MostrarMsg($"Erro ao configurar conexão: {ex.Message}", sucesso: false);
            }
        }

        // Select *
        private void CarregarAlunos()
        {
            try
            {
                using var conn = new MySqlConnection(_connectionString);
                conn.Open();

                string sql = $"SELECT id, nome, idade FROM {Table} ORDER BY id;";
                using var cmd = new MySqlCommand(sql, conn);
                using var reader = cmd.ExecuteReader();

                var lista = new List<dynamic>();
                while (reader.Read())
                {
                    lista.Add(new
                    {
                        id = reader["id"],
                        nome = reader["nome"].ToString(),
                        idade = reader["idade"]
                    });
                }

                GridAlunos.ItemsSource = lista;
                AtualizarContador(lista.Count);
                AtualizarAvisoVazio(lista.Count);
            }
            catch (Exception ex)
            {
                MostrarMsg($"Erro ao carregar dados: {ex.Message}", sucesso: false);
            }
        }

        // Select por ID
        private void BtnBuscar_Click(object sender, RoutedEventArgs e) => BuscarPorId();

        private void TxtBuscaId_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) BuscarPorId();
        }

        private void BuscarPorId()
        {
            var texto = TxtBuscaId.Text.Trim();
            if (string.IsNullOrWhiteSpace(texto))
            {
                MostrarMsg("Digite um ID para buscar.", sucesso: false);
                return;
            }

            if (!int.TryParse(texto, out int id))
            {
                MostrarMsg("O ID deve ser um número inteiro.", sucesso: false);
                return;
            }

            try
            {
                using var conn = new MySqlConnection(_connectionString);
                conn.Open();

                string sql = $"SELECT id, nome, idade FROM {Table} WHERE id = @id;";
                using var cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@id", id);
                using var reader = cmd.ExecuteReader();

                var lista = new List<dynamic>();
                while (reader.Read())
                {
                    lista.Add(new
                    {
                        id = reader["id"],
                        nome = reader["nome"].ToString(),
                        idade = reader["idade"]
                    });
                }

                GridAlunos.ItemsSource = lista;
                AtualizarContador(lista.Count, filtrado: true);

                if (lista.Count == 0)
                {
                    AtualizarAvisoVazio(0, mensagemBusca: $"Nenhum aluno com ID {id}.");
                    MostrarMsg($"Nenhum aluno encontrado com ID {id}.", sucesso: false);
                }
                else
                {
                    AtualizarAvisoVazio(lista.Count);
                    MostrarMsg($"Aluno de ID {id} encontrado.", sucesso: true);
                }
            }
            catch (Exception ex)
            {
                MostrarMsg($"Erro na busca: {ex.Message}", sucesso: false);
            }
        }

        private void BtnVerTodos_Click(object sender, RoutedEventArgs e)
        {
            TxtBuscaId.Text = string.Empty;
            CarregarAlunos();
            PanelMsg.Visibility = Visibility.Collapsed;
        }

        // Insert
        private void BtnInserir_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidarNomeIdade(out string nome, out int idade)) return;

            try
            {
                App.InsertOperation(new MySqlConnection(_connectionString), Table, nome, idade.ToString());
                CarregarAlunos();
                LimparCampos();
                MostrarMsg("Aluno inserido com sucesso!", sucesso: true);
            }
            catch (Exception ex)
            {
                MostrarMsg($"Erro ao inserir: {ex.Message}", sucesso: false);
            }
        }

        // Update
        private void BtnAtualizar_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtId.Text))
            {
                MostrarMsg("Selecione um aluno na tabela para atualizar.", sucesso: false);
                return;
            }

            if (!ValidarNomeIdade(out string nome, out int idade)) return;

            try
            {
                App.UpdateOperation(
                    new MySqlConnection(_connectionString),
                    TxtId.Text.Trim(), Table, nome, idade.ToString());

                CarregarAlunos();
                LimparCampos();
                MostrarMsg("Aluno atualizado com sucesso!", sucesso: true);
            }
            catch (Exception ex)
            {
                MostrarMsg($"Erro ao atualizar: {ex.Message}", sucesso: false);
            }
        }

        // Delete
        private void BtnDeletar_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtId.Text))
            {
                MostrarMsg("Selecione um aluno na tabela para deletar.", sucesso: false);
                return;
            }

            var confirm = MessageBox.Show(
                $"Deseja realmente deletar o aluno de ID {TxtId.Text}?",
                "Confirmar exclusão",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (confirm != MessageBoxResult.Yes) return;

            try
            {
                App.DeleteOperation(
                    new MySqlConnection(_connectionString),
                    TxtId.Text.Trim(), Table);

                CarregarAlunos();
                LimparCampos();
                MostrarMsg("Aluno deletado com sucesso!", sucesso: true);
            }
            catch (Exception ex)
            {
                MostrarMsg($"Erro ao deletar: {ex.Message}", sucesso: false);
            }
        }

        // Dar refresh no Select
        private void BtnRecarregar_Click(object sender, RoutedEventArgs e)
        {
            TxtBuscaId.Text = string.Empty;
            CarregarAlunos();
            MostrarMsg("Lista recarregada.", sucesso: true);
        }

        // Limpar forms
        private void BtnLimpar_Click(object sender, RoutedEventArgs e)
        {
            LimparCampos();
            PanelMsg.Visibility = Visibility.Collapsed;
        }
        private void LimparCampos()
        {
            TxtId.Text = string.Empty;
            TxtNome.Text = string.Empty;
            TxtIdade.Text = string.Empty;
            GridAlunos.SelectedItem = null;
        }

        // Atalho para update (pegar informacoes ao selecionar)
        private void GridAlunos_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (GridAlunos.SelectedItem is not { } item) return;

            var tipo = item.GetType();
            TxtId.Text = tipo.GetProperty("id")?.GetValue(item)?.ToString() ?? "";
            TxtNome.Text = tipo.GetProperty("nome")?.GetValue(item)?.ToString() ?? "";
            TxtIdade.Text = tipo.GetProperty("idade")?.GetValue(item)?.ToString() ?? "";
        }

        // Validacao dos forms
        private void AtualizarContador(int count, bool filtrado = false)
        {
            if (filtrado && count > 0)
                TxtContagem.Text = "1 resultado";
            else
                TxtContagem.Text = $"{count} registro{(count != 1 ? "s" : "")}";
        }
        private void AtualizarAvisoVazio(int count, string? mensagemBusca = null)
        {
            if (count == 0)
            {
                PanelVazio.Visibility = Visibility.Visible;
                if (mensagemBusca != null)
                {
                    TxtVazio.Text = mensagemBusca;
                    TxtVazioSub.Text = "Tente outro ID ou clique em \"Ver Todos\".";
                }
                else
                {
                    TxtVazio.Text = "Nenhum aluno cadastrado";
                    TxtVazioSub.Text = "Insira um aluno pelo formulário ao lado.";
                }
            }
            else
            {
                PanelVazio.Visibility = Visibility.Collapsed;
            }
        }
        private bool ValidarNomeIdade(out string nome, out int idade)
        {
            nome = TxtNome.Text.Trim();
            idade = 0;

            if (string.IsNullOrWhiteSpace(nome))
            {
                MostrarMsg("O campo Nome é obrigatório.", sucesso: false);
                return false;
            }

            if (!int.TryParse(TxtIdade.Text.Trim(), out idade) || idade <= 0)
            {
                MostrarMsg("Informe uma Idade válida (número inteiro positivo).", sucesso: false);
                return false;
            }

            return true;
        }
        private void MostrarMsg(string texto, bool sucesso)
        {
            TxtMsg.Text = texto;

            if (sucesso)
            {
                PanelMsg.Background = new SolidColorBrush(Color.FromArgb(30, 0, 212, 255));
                PanelMsg.BorderBrush = new SolidColorBrush(Color.FromArgb(80, 0, 212, 255));
                PanelMsg.BorderThickness = new Thickness(1);
                TxtMsg.Foreground = new SolidColorBrush(Color.FromRgb(0, 212, 255));
            }
            else
            {
                PanelMsg.Background = new SolidColorBrush(Color.FromArgb(30, 255, 45, 120));
                PanelMsg.BorderBrush = new SolidColorBrush(Color.FromArgb(80, 255, 45, 120));
                PanelMsg.BorderThickness = new Thickness(1);
                TxtMsg.Foreground = new SolidColorBrush(Color.FromRgb(255, 45, 120));
            }

            PanelMsg.Visibility = Visibility.Visible;
        }

        // Status da conexao com mysql
        private void MostrarStatus(bool conectado)
        {
            TxtStatus.Text = conectado ? "Conectado" : "Sem conexão";
        }
    }
}