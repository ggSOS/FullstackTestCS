using Microsoft.Extensions.Configuration;
using MySqlConnector;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FullstackTestCS
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
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

        // ─────────────────────────────────────────────
        // Inicialização
        // ─────────────────────────────────────────────
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

        // ─────────────────────────────────────────────
        // Carregar / SELECT
        // ─────────────────────────────────────────────
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
                TxtContagem.Text = $"{lista.Count} registro{(lista.Count != 1 ? "s" : "")}";
            }
            catch (Exception ex)
            {
                MostrarMsg($"Erro ao carregar dados: {ex.Message}", sucesso: false);
            }
        }

        // ─────────────────────────────────────────────
        // Inserir
        // ─────────────────────────────────────────────
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

        // ─────────────────────────────────────────────
        // Atualizar
        // ─────────────────────────────────────────────
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

        // ─────────────────────────────────────────────
        // Deletar
        // ─────────────────────────────────────────────
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

        // ─────────────────────────────────────────────
        // Recarregar
        // ─────────────────────────────────────────────
        private void BtnRecarregar_Click(object sender, RoutedEventArgs e)
        {
            CarregarAlunos();
            MostrarMsg("Lista recarregada.", sucesso: true);
        }

        // ─────────────────────────────────────────────
        // Limpar campos
        // ─────────────────────────────────────────────
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

        // ─────────────────────────────────────────────
        // Seleção na DataGrid → preenche formulário
        // ─────────────────────────────────────────────
        private void GridAlunos_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (GridAlunos.SelectedItem is not { } item) return;

            // Acessa as propriedades via reflexão (tipo anônimo dinâmico)
            var tipo = item.GetType();
            TxtId.Text = tipo.GetProperty("id")?.GetValue(item)?.ToString() ?? "";
            TxtNome.Text = tipo.GetProperty("nome")?.GetValue(item)?.ToString() ?? "";
            TxtIdade.Text = tipo.GetProperty("idade")?.GetValue(item)?.ToString() ?? "";
        }

        // ─────────────────────────────────────────────
        // Helpers de UI
        // ─────────────────────────────────────────────
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

        private void MostrarStatus(bool conectado)
        {
            TxtStatus.Text = conectado ? "Conectado" : "Sem conexão";
        }
    }
}