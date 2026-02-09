using System.Threading.Tasks;
using MedicalSystem.App.Demos;
using MedicalSystem.App.UI.Menus;

namespace MedicalSystem.App.UI
{
    public class MenuSystem
    {
        private readonly string _connectionString;

        public MenuSystem(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task RunAsync()
        {
            bool exit = false;

            while (!exit)
            {
                ShowMainMenu();
                var choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        var crudMenu = new CrudManagementMenu(_connectionString);
                        await crudMenu.RunAsync();
                        break;

                    case "2":
                        await new RelatedDataDemo(_connectionString).RunAsync();
                        break;

                    case "3":
                        await new MigrationInfoDemo(_connectionString).RunAsync();
                        break;

                    case "4":
                        await new CustomOrmBasicDemo(_connectionString).RunAsync();
                        break;

                    case "5":
                        await new CustomOrmChangeTrackingDemo(_connectionString).RunAsync();
                        break;

                    case "6":
                        await new CustomOrmExpressionDemo(_connectionString).RunAsync();
                        break;

                    case "7":
                        await new CustomOrmMigrationDemo(_connectionString).RunAsync();
                        break;

                    case "0":
                        exit = true;
                        Console.WriteLine(" Goodbye!");
                        break;

                    default:
                        Console.WriteLine("❌ Invalid option");
                        Console.ReadKey();
                        break;
                }
            }
        }

        private void ShowMainMenu()
        {
            Console.Clear();
            Console.WriteLine("╔════════════════════════════════════════════════╗");
            Console.WriteLine("║ MAIN MENU                                     ║");
            Console.WriteLine("╠════════════════════════════════════════════════╣");
            Console.WriteLine("║ EF CORE:                                      ║");
            Console.WriteLine("║ 1. CRUD Management                            ║");
            Console.WriteLine("║ 2. Related Data Demo (Eager Loading)          ║");
            Console.WriteLine("║ 3. Migration Info                             ║");
            Console.WriteLine("╠════════════════════════════════════════════════╣");
            Console.WriteLine("║ CUSTOM ORM DEMOS:                             ║");
            Console.WriteLine("║ 4. Basic CRUD Demo                            ║");
            Console.WriteLine("║ 5. Change Tracking Demo                       ║");
            Console.WriteLine("║ 6. Expression & Filtering Demo                ║");
            Console.WriteLine("║ 7. Migration System Demo                      ║");
            Console.WriteLine("╠════════════════════════════════════════════════╣");
            Console.WriteLine("║ 0. Exit                                       ║");
            Console.WriteLine("╚════════════════════════════════════════════════╝");
            Console.Write("\nSelect option: ");
        }
    }
}