using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Linq; // Для Linq (например, для объединения даты и времени, Max ID)

// Добавьте эту ссылку в проекте, если ее нет:
// References -> Add Reference -> Assemblies -> Framework -> Microsoft.VisualBasic
// Это нужно для использования Interaction.InputBox
using Microsoft.VisualBasic;

namespace STORegistration
{
    // --- Класс для представления Записи на сервис ---
    // Этот класс должен быть определен в вашем проекте
    public class Appointment
    {
        public int Id { get; set; } // Для уникальности
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Car { get; set; } // Марка авто
        public string Service { get; set; }
        public DateTime AppointmentDateTime { get; set; } // Объединим дату и время
        public string Status { get; set; } = "Новая"; // Например: Новая, В работе, Завершена, Отменена

        // Переопределяем ToString для удобного отображения в ListBox
        public override string ToString()
        {
            // Проверка на нулевую дату, если она не была установлена (хотя в коде мы ее устанавливаем)
            if (AppointmentDateTime == DateTime.MinValue)
            {
                return $"{Name}, {Phone}, {Car}, {Service} (Дата не установлена)";
            }
            return $"{AppointmentDateTime:dd.MM.yyyy HH:mm} - {Name}, {Phone}, {Car}, {Service} ({Status})";
        }
    }

    public partial class Form1 : Form
    {
        // Список для хранения всех записей в памяти. Данные будут теряться при закрытии.
        List<Appointment> appointments = new List<Appointment>();

        // Пароль администратора
        private string adminPassword = "admin123"; // Можно заменить на любой пароль

        public Form1() => InitializeComponent(); // Инициализация элементов, созданных в дизайнере (если есть)

        private void Form1_Load(object sender, EventArgs e)
        {
            // Настройки главной формы
            this.Text = "Регистрация на СТО";
            this.Size = new Size(650, 550);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(240, 240, 240);

            Font mainFont = new Font("Segoe UI", 10, FontStyle.Regular);

            // --- Панель с полями ввода ---
            TableLayoutPanel panel = new TableLayoutPanel();
            panel.Dock = DockStyle.Top;
            panel.RowCount = 6; // 6 строк для полей: Имя, Телефон, Авто, Услуга, Дата, Время
            panel.ColumnCount = 2;
            panel.Padding = new Padding(20);
            panel.AutoSize = true;
            panel.BackColor = Color.FromArgb(245, 245, 245);
            panel.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;

            // Поля для регистрации
            panel.Controls.Add(new Label() { Text = "Имя:", AutoSize = true, Font = mainFont, Margin = new Padding(0, 5, 0, 0) }, 0, 0);
            TextBox textBoxName = new TextBox() { Name = "textBoxName", Font = mainFont, Dock = DockStyle.Fill, Margin = new Padding(5) };
            panel.Controls.Add(textBoxName, 1, 0);

            panel.Controls.Add(new Label() { Text = "Телефон:", AutoSize = true, Font = mainFont, Margin = new Padding(0, 5, 0, 0) }, 0, 1);
            TextBox textBoxPhone = new TextBox() { Name = "textBoxPhone", Font = mainFont, Dock = DockStyle.Fill, Margin = new Padding(5) };
            panel.Controls.Add(textBoxPhone, 1, 1);

            panel.Controls.Add(new Label() { Text = "Марка авто:", AutoSize = true, Font = mainFont, Margin = new Padding(0, 5, 0, 0) }, 0, 2);
            TextBox textBoxCar = new TextBox() { Name = "textBoxCar", Font = mainFont, Dock = DockStyle.Fill, Margin = new Padding(5) };
            panel.Controls.Add(textBoxCar, 1, 2);

            panel.Controls.Add(new Label() { Text = "Услуга:", AutoSize = true, Font = mainFont, Margin = new Padding(0, 5, 0, 0) }, 0, 3);
            ComboBox comboBoxService = new ComboBox() { Name = "comboBoxService", Font = mainFont, DropDownStyle = ComboBoxStyle.DropDownList, Dock = DockStyle.Fill, Margin = new Padding(5) };
            comboBoxService.Items.AddRange(new string[] { "Замена масла", "Ремонт двигателя", "Шиномонтаж", "Диагностика", "Замена тормозных колодок" });
            panel.Controls.Add(comboBoxService, 1, 3);

            // Поля для даты и времени
            panel.Controls.Add(new Label() { Text = "Дата:", AutoSize = true, Font = mainFont, Margin = new Padding(0, 5, 0, 0) }, 0, 4);
            DateTimePicker dateTimePickerDate = new DateTimePicker() { Name = "dateTimePickerDate", Format = DateTimePickerFormat.Short, Font = mainFont, Dock = DockStyle.Fill, Margin = new Padding(5) };
            panel.Controls.Add(dateTimePickerDate, 1, 4);

            panel.Controls.Add(new Label() { Text = "Время:", AutoSize = true, Font = mainFont, Margin = new Padding(0, 5, 0, 0) }, 0, 5);
            DateTimePicker dateTimePickerTime = new DateTimePicker() { Name = "dateTimePickerTime", Format = DateTimePickerFormat.Time, ShowUpDown = true, Font = mainFont, Dock = DockStyle.Fill, Margin = new Padding(5) };
            panel.Controls.Add(dateTimePickerTime, 1, 5);

            this.Controls.Add(panel); // Добавляем панель с полями на форму

            // --- Кнопка "Записаться" ---
            Button buttonAdd = new Button()
            {
                Text = "Записаться",
                Dock = DockStyle.Top,
                Height = 45,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                BackColor = Color.FromArgb(0, 120, 215), // Синий цвет
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(10, 5, 10, 5)
            };
            buttonAdd.FlatAppearance.BorderSize = 0;
            this.Controls.Add(buttonAdd);

            // --- Кнопка "Посмотреть всех" ---
            Button buttonShowAll = new Button()
            {
                Text = "Посмотреть все записи", // Изменил текст для ясности
                Dock = DockStyle.Top,
                Height = 45,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                BackColor = Color.FromArgb(50, 150, 50), // Зеленый цвет
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(10, 5, 10, 5)
            };
            buttonShowAll.FlatAppearance.BorderSize = 0;
            this.Controls.Add(buttonShowAll);

            // --- Список для отображения записей ---
            ListBox listBoxAppointments = new ListBox()
            {
                Name = "listBoxAppointments",
                Dock = DockStyle.Fill,
                Font = mainFont,
                BackColor = Color.WhiteSmoke,
                Margin = new Padding(10)
            };
            this.Controls.Add(listBoxAppointments);

            // --- Обработчик кнопки "Записаться" ---
            buttonAdd.Click += (s, ev) =>
            {
                // Валидация полей
                if (string.IsNullOrWhiteSpace(textBoxName.Text) ||
                    string.IsNullOrWhiteSpace(textBoxPhone.Text) ||
                    string.IsNullOrWhiteSpace(textBoxCar.Text) ||
                    comboBoxService.SelectedIndex == -1 || // Проверяем, выбрана ли услуга
                    dateTimePickerDate.Value.Date < DateTime.Today || // Дата не может быть в прошлом
                    (dateTimePickerDate.Value.Date == DateTime.Today && dateTimePickerTime.Value.TimeOfDay < DateTime.Now.TimeOfDay)) // Время не может быть в прошлом, если сегодня
                {
                    MessageBox.Show("Пожалуйста, заполните все поля корректно. Дата и время не могут быть в прошлом.", "Ошибка ввода", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Объединяем дату и время
                DateTime appointmentDateTime = dateTimePickerDate.Value.Date + dateTimePickerTime.Value.TimeOfDay;

                // Создание нового объекта Appointment
                Appointment appt = new Appointment()
                {
                    // Генерируем простой ID, если список не пустой. Если пустой, ID будет 1.
                    Id = appointments.Count > 0 ? appointments.Max(a => a.Id) + 1 : 1,
                    Name = textBoxName.Text.Trim(), // Убираем лишние пробелы
                    Phone = textBoxPhone.Text.Trim(),
                    Car = textBoxCar.Text.Trim(),
                    Service = comboBoxService.SelectedItem.ToString(),
                    AppointmentDateTime = appointmentDateTime,
                    Status = "Новая" // Устанавливаем начальный статус
                };

                appointments.Add(appt);
                // Обновляем ListBox, используя метод ToString() объекта Appointment
                listBoxAppointments.Items.Add(appt.ToString());

                // Очистка полей ввода после успешной записи
                textBoxName.Clear();
                textBoxPhone.Clear();
                textBoxCar.Clear();
                comboBoxService.SelectedIndex = -1;
                dateTimePickerDate.Value = DateTime.Today; // Сбрасываем на текущую дату
                dateTimePickerTime.Value = DateTime.Now; // Сбрасываем на текущее время

                MessageBox.Show("Запись успешно добавлена!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // *** Логика сохранения данных отсутствует, т.к. они хранятся только в памяти ***
            };

            // --- Обработчик кнопки "Посмотреть все" ---
            buttonShowAll.Click += (s, ev) =>
            {
                // Проверяем, есть ли вообще записи
                if (appointments.Count == 0)
                {
                    MessageBox.Show("Нет зарегистрированных клиентов.", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Запрос пароля администратора
                // Требуется ссылка на сборку Microsoft.VisualBasic
                string input = Microsoft.VisualBasic.Interaction.InputBox("Введите пароль администратора:", "Доступ администратора", "", -1, -1);

                // Проверка пароля
                if (string.IsNullOrWhiteSpace(input) || input != adminPassword)
                {
                    MessageBox.Show("Неверный пароль!", "Ошибка доступа", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Создание нового окна для отображения всех записей
                Form showForm = new Form();
                showForm.Text = "Все зарегистрированные записи";
                showForm.Size = new Size(600, 450);
                showForm.StartPosition = FormStartPosition.CenterParent;
                showForm.BackColor = Color.FromArgb(240, 240, 240);

                // ListBox для отображения всех записей
                ListBox allList = new ListBox()
                {
                    Dock = DockStyle.Fill,
                    Font = mainFont,
                    BackColor = Color.WhiteSmoke,
                    Margin = new Padding(10)
                };

                // Заполняем ListBox данными из списка appointments
                foreach (var appt in appointments)
                {
                    allList.Items.Add(appt.ToString()); // Используем метод ToString() класса Appointment
                }

                showForm.Controls.Add(allList);
                showForm.ShowDialog(); // Показываем модальное окно
            };
        }

        // --- Обработчик закрытия формы ---
        // В этой версии кода, при закрытии формы данные НЕ СОХРАНЯЮТСЯ.
        // Список appointments просто очистится при следующем запуске.
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Удален вызов SaveAppointments(), т.к. данные не сохраняются.
            // Если бы мы использовали сохранение, оно было бы здесь.
        }
    }
}