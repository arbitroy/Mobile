using Android.App;
using Android.OS;
using Android.Widget;
using System;
using System.Collections.Generic;
using Mobile.Models;
using Mobile.Services;
using Android.Content;
using Android.Views;
using AndroidX.RecyclerView.Widget;
using System.Linq;

namespace Mobile.Activities
{
    [Activity(Label = "Edit Quiz")]
    public class AdminEditQuizActivity : BaseAuthenticatedActivity
    {
        private EditText _titleEditText;
        private EditText _descriptionEditText;
        private EditText _timeLimitEditText;
        private Button _addQuestionButton;
        private RecyclerView _questionsRecyclerView;
        private Button _saveQuizButton;
        private Button _cancelButton;
        private ProgressBar _loadingProgressBar;

        private int _quizId;
        private QuizAdmin _quiz;
        private QuestionAdapter _adapter;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the layout resource
            SetContentView(Resource.Layout.activity_admin_edit_quiz);

            // Get quiz ID from intent
            _quizId = Intent.GetIntExtra("QuizId", 0);
            if (_quizId == 0)
            {
                Toast.MakeText(this, "Invalid quiz ID", ToastLength.Short).Show();
                Finish();
                return;
            }

            // Initialize UI elements
            _titleEditText = FindViewById<EditText>(Resource.Id.titleEditText);
            _descriptionEditText = FindViewById<EditText>(Resource.Id.descriptionEditText);
            _timeLimitEditText = FindViewById<EditText>(Resource.Id.timeLimitEditText);
            _addQuestionButton = FindViewById<Button>(Resource.Id.addQuestionButton);
            _questionsRecyclerView = FindViewById<RecyclerView>(Resource.Id.questionsRecyclerView);
            _saveQuizButton = FindViewById<Button>(Resource.Id.saveQuizButton);
            _cancelButton = FindViewById<Button>(Resource.Id.cancelButton);
            _loadingProgressBar = FindViewById<ProgressBar>(Resource.Id.loadingProgressBar);

            // Set up RecyclerView
            _questionsRecyclerView.SetLayoutManager(new LinearLayoutManager(this));

            // Load quiz details
            LoadQuizAsync();

            // Set up event handlers
            _addQuestionButton.Click += OnAddQuestionButtonClick;
            _saveQuizButton.Click += OnSaveQuizButtonClick;
            _cancelButton.Click += OnCancelButtonClick;
        }

        protected override void OnRolesLoaded()
        {
            // Ensure user has admin access
            if (!IsAdmin)
            {
                Toast.MakeText(this, "You need administrator privileges to access this page", ToastLength.Long).Show();
                Finish();
            }
        }

        private async void LoadQuizAsync()
        {
            try
            {
                // Show loading indicator
                _loadingProgressBar.Visibility = ViewStates.Visible;

                // Get quiz details from API
                _quiz = await ApiService.GetQuizAdminAsync(_quizId);

                // Update UI with quiz details
                _titleEditText.Text = _quiz.Title;
                _descriptionEditText.Text = _quiz.Description;
                _timeLimitEditText.Text = _quiz.TimeLimit.ToString();

                // Set up adapter
                _adapter = new QuestionAdapter(this, _quiz.Questions);
                _questionsRecyclerView.SetAdapter(_adapter);
            }
            catch (UnauthorizedAccessException)
            {
                HandleAuthError();
            }
            catch (Exception ex)
            {
                Toast.MakeText(this, $"Failed to load quiz: {ex.Message}", ToastLength.Long).Show();
                Finish();
            }
            finally
            {
                // Hide loading indicator
                _loadingProgressBar.Visibility = ViewStates.Gone;
            }
        }

        private void OnAddQuestionButtonClick(object sender, EventArgs e)
        {
            ShowAddQuestionDialog();
        }

        private async void OnSaveQuizButtonClick(object sender, EventArgs e)
        {
            try
            {
                // Validate quiz data
                string title = _titleEditText.Text?.Trim();
                if (string.IsNullOrEmpty(title))
                {
                    Toast.MakeText(this, "Quiz title is required", ToastLength.Short).Show();
                    return;
                }

                if (!int.TryParse(_timeLimitEditText.Text, out int timeLimit) || timeLimit <= 0)
                {
                    Toast.MakeText(this, "Please enter a valid time limit in minutes", ToastLength.Short).Show();
                    return;
                }

                if (_quiz.Questions.Count == 0)
                {
                    Toast.MakeText(this, "Please add at least one question", ToastLength.Short).Show();
                    return;
                }

                // Check if all questions have at least 2 options and a correct answer
                foreach (var question in _quiz.Questions)
                {
                    if (question.Options.Count < 2)
                    {
                        Toast.MakeText(this, $"Question '{question.Text}' must have at least 2 options", ToastLength.Short).Show();
                        return;
                    }

                    if (!question.Options.Any(o => o.IsCorrect))
                    {
                        Toast.MakeText(this, $"Question '{question.Text}' must have a correct answer", ToastLength.Short).Show();
                        return;
                    }
                }

                // Update quiz data
                _quiz.Title = title;
                _quiz.Description = _descriptionEditText.Text?.Trim() ?? "";
                _quiz.TimeLimit = timeLimit;

                // Show loading indicator
                _loadingProgressBar.Visibility = ViewStates.Visible;
                _saveQuizButton.Enabled = false;

                // Update quiz via API
                await ApiService.UpdateQuizAsync(_quizId, _quiz);

                Toast.MakeText(this, "Quiz updated successfully", ToastLength.Short).Show();
                Finish(); // Return to previous activity
            }
            catch (UnauthorizedAccessException)
            {
                // Handle authentication errors
                HandleAuthError();
            }
            catch (Exception ex)
            {
                Toast.MakeText(this, $"Failed to update quiz: {ex.Message}", ToastLength.Long).Show();
            }
            finally
            {
                // Hide loading indicator
                _loadingProgressBar.Visibility = ViewStates.Gone;
                _saveQuizButton.Enabled = true;
            }
        }

        private void OnCancelButtonClick(object sender, EventArgs e)
        {
            // Show confirmation dialog
            var alertDialog = new AlertDialog.Builder(this);
            alertDialog.SetTitle("Discard Changes");
            alertDialog.SetMessage("Are you sure you want to discard your changes?");
            alertDialog.SetPositiveButton("Discard", (senderAlert, args) => {
                Finish(); // Return to previous activity
            });
            alertDialog.SetNegativeButton("Keep Editing", (senderAlert, args) => {
                // Do nothing
            });
            alertDialog.Show();
        }

        private void ShowAddQuestionDialog()
        {
            // Create a dialog for adding a new question
            var dialogView = LayoutInflater.From(this).Inflate(Resource.Layout.dialog_add_question, null);
            var questionEditText = dialogView.FindViewById<EditText>(Resource.Id.questionEditText);

            var alertDialog = new AlertDialog.Builder(this);
            alertDialog.SetTitle("Add Question");
            alertDialog.SetView(dialogView);
            alertDialog.SetPositiveButton("Add", (senderAlert, args) => {
                string questionText = questionEditText.Text?.Trim();
                if (!string.IsNullOrEmpty(questionText))
                {
                    // Create new question
                    var question = new QuestionAdmin
                    {
                        Text = questionText,
                        Options = new List<OptionAdmin>()
                    };

                    // Add to quiz
                    _quiz.Questions.Add(question);
                    _adapter.NotifyItemInserted(_quiz.Questions.Count - 1);

                    // Show add options dialog
                    ShowAddOptionDialog(question);
                }
                else
                {
                    Toast.MakeText(this, "Question text is required", ToastLength.Short).Show();
                }
            });
            alertDialog.SetNegativeButton("Cancel", (senderAlert, args) => {
                // Do nothing
            });
            alertDialog.Show();
        }

        private void ShowAddOptionDialog(QuestionAdmin question)
        {
            // Create a dialog for adding an option
            var dialogView = LayoutInflater.From(this).Inflate(Resource.Layout.dialog_add_option, null);
            var optionEditText = dialogView.FindViewById<EditText>(Resource.Id.optionEditText);
            var isCorrectCheckBox = dialogView.FindViewById<CheckBox>(Resource.Id.isCorrectCheckBox);

            var alertDialog = new AlertDialog.Builder(this);
            alertDialog.SetTitle("Add Option");
            alertDialog.SetView(dialogView);
            alertDialog.SetPositiveButton("Add", (senderAlert, args) => {
                string optionText = optionEditText.Text?.Trim();
                if (!string.IsNullOrEmpty(optionText))
                {
                    // Create new option
                    var option = new OptionAdmin
                    {
                        Text = optionText,
                        IsCorrect = isCorrectCheckBox.Checked
                    };

                    // If this is marked as correct, unmark any other correct options
                    if (option.IsCorrect)
                    {
                        foreach (var existingOption in question.Options)
                        {
                            existingOption.IsCorrect = false;
                        }
                    }

                    // Add to question
                    question.Options.Add(option);
                    _adapter.NotifyDataSetChanged();

                    // Ask if user wants to add another option
                    ShowAddAnotherOptionDialog(question);
                }
                else
                {
                    Toast.MakeText(this, "Option text is required", ToastLength.Short).Show();
                    ShowAddOptionDialog(question); // Show dialog again
                }
            });
            alertDialog.SetNegativeButton("Cancel", (senderAlert, args) => {
                // If no options were added, remove the question
                if (question.Options.Count == 0)
                {
                    int index = _quiz.Questions.IndexOf(question);
                    if (index != -1)
                    {
                        _quiz.Questions.RemoveAt(index);
                        _adapter.NotifyItemRemoved(index);
                    }
                }
            });
            alertDialog.Show();
        }

        private void ShowAddAnotherOptionDialog(QuestionAdmin question)
        {
            var alertDialog = new AlertDialog.Builder(this);
            alertDialog.SetTitle("Add Another Option");
            alertDialog.SetMessage("Would you like to add another option to this question?");
            alertDialog.SetPositiveButton("Yes", (senderAlert, args) => {
                ShowAddOptionDialog(question);
            });
            alertDialog.SetNegativeButton("No", (senderAlert, args) => {
                // Check if a correct answer was selected
                if (!question.Options.Any(o => o.IsCorrect) && question.Options.Count > 0)
                {
                    Toast.MakeText(this, "Please mark at least one option as correct", ToastLength.Long).Show();

                    // Show dialog to select correct answer
                    ShowSelectCorrectAnswerDialog(question);
                }
            });
            alertDialog.Show();
        }

        private void ShowSelectCorrectAnswerDialog(QuestionAdmin question)
        {
            // Create array of option texts
            string[] options = question.Options.Select(o => o.Text).ToArray();

            var alertDialog = new AlertDialog.Builder(this);
            alertDialog.SetTitle("Select Correct Answer");
            alertDialog.SetSingleChoiceItems(options, -1, (sender, args) => {
                // Mark selected option as correct
                for (int i = 0; i < question.Options.Count; i++)
                {
                    question.Options[i].IsCorrect = (i == args.Which);
                }
                _adapter.NotifyDataSetChanged();

                // Dismiss dialog
                ((AlertDialog)sender).Dismiss();
            });
            alertDialog.SetCancelable(false); // Force selection
            alertDialog.Show();
        }

        private void HandleAuthError()
        {
            TokenManager.ClearToken(this);
            Toast.MakeText(this, "Your session has expired. Please log in again.", ToastLength.Long).Show();
            RedirectToLogin();
        }

        // RecyclerView adapter for questions
        private class QuestionAdapter : RecyclerView.Adapter
        {
            private readonly AdminEditQuizActivity _activity;
            private readonly List<QuestionAdmin> _questions;

            public QuestionAdapter(AdminEditQuizActivity activity, List<QuestionAdmin> questions)
            {
                _activity = activity;
                _questions = questions;
            }

            public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
            {
                View itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.item_admin_question, parent, false);
                return new QuestionViewHolder(itemView);
            }

            public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
            {
                QuestionViewHolder viewHolder = holder as QuestionViewHolder;
                QuestionAdmin question = _questions[position];

                // Set question number and text
                viewHolder.QuestionNumberTextView.Text = $"Question {position + 1}";
                viewHolder.QuestionTextView.Text = question.Text;

                // Clear options container and add option views
                viewHolder.OptionsContainer.RemoveAllViews();
                foreach (var option in question.Options)
                {
                    // Create option view
                    var optionView = LayoutInflater.From(_activity).Inflate(Resource.Layout.item_admin_option, viewHolder.OptionsContainer, false);
                    var optionTextView = optionView.FindViewById<TextView>(Resource.Id.optionTextView);
                    var correctIndicator = optionView.FindViewById<ImageView>(Resource.Id.correctIndicator);
                    var editOptionButton = optionView.FindViewById<ImageButton>(Resource.Id.editOptionButton);
                    var deleteOptionButton = optionView.FindViewById<ImageButton>(Resource.Id.deleteOptionButton);

                    // Set option text and correct indicator
                    optionTextView.Text = option.Text;
                    correctIndicator.Visibility = option.IsCorrect ? ViewStates.Visible : ViewStates.Gone;

                    // Set up edit option button
                    editOptionButton.Click += (sender, e) => {
                        ShowEditOptionDialog(question, option);
                    };

                    // Set up delete option button
                    deleteOptionButton.Click += (sender, e) => {
                        ShowDeleteOptionConfirmation(question, option);
                    };

                    // Set up click to toggle correct
                    optionView.Click += (sender, e) => {
                        // If this option is already correct, do nothing
                        if (option.IsCorrect)
                            return;

                        // Otherwise, set this option as correct and others as incorrect
                        foreach (var opt in question.Options)
                        {
                            opt.IsCorrect = (opt == option);
                        }
                        NotifyItemChanged(position);
                    };

                    // Add to container
                    viewHolder.OptionsContainer.AddView(optionView);
                }

                // Set up edit button
                viewHolder.EditButton.Click += (sender, e) => {
                    ShowEditQuestionDialog(question, position);
                };

                // Set up delete button
                viewHolder.DeleteButton.Click += (sender, e) => {
                    ShowDeleteQuestionConfirmation(question, position);
                };

                // Set up add option button
                viewHolder.AddOptionButton.Click += (sender, e) => {
                    _activity.ShowAddOptionDialog(question);
                };
            }

            public override int ItemCount => _questions.Count;

            private void ShowEditQuestionDialog(QuestionAdmin question, int position)
            {
                // Create a dialog for editing the question
                var dialogView = LayoutInflater.From(_activity).Inflate(Resource.Layout.dialog_add_question, null);
                var questionEditText = dialogView.FindViewById<EditText>(Resource.Id.questionEditText);
                questionEditText.Text = question.Text;

                var alertDialog = new AlertDialog.Builder(_activity);
                alertDialog.SetTitle("Edit Question");
                alertDialog.SetView(dialogView);
                alertDialog.SetPositiveButton("Save", (senderAlert, args) => {
                    string questionText = questionEditText.Text?.Trim();
                    if (!string.IsNullOrEmpty(questionText))
                    {
                        // Update question text
                        question.Text = questionText;
                        NotifyItemChanged(position);
                    }
                    else
                    {
                        Toast.MakeText(_activity, "Question text is required", ToastLength.Short).Show();
                    }
                });
                alertDialog.SetNegativeButton("Cancel", (senderAlert, args) => {
                    // Do nothing
                });
                alertDialog.Show();
            }

            private void ShowDeleteQuestionConfirmation(QuestionAdmin question, int position)
            {
                var alertDialog = new AlertDialog.Builder(_activity);
                alertDialog.SetTitle("Delete Question");
                alertDialog.SetMessage("Are you sure you want to delete this question?");
                alertDialog.SetPositiveButton("Delete", (senderAlert, args) => {
                    // Remove question
                    _questions.RemoveAt(position);
                    NotifyItemRemoved(position);
                    NotifyItemRangeChanged(position, _questions.Count - position);
                });
                alertDialog.SetNegativeButton("Cancel", (senderAlert, args) => {
                    // Do nothing
                });
                alertDialog.Show();
            }

            private void ShowEditOptionDialog(QuestionAdmin question, OptionAdmin option)
            {
                // Create a dialog for editing an option
                var dialogView = LayoutInflater.From(_activity).Inflate(Resource.Layout.dialog_add_option, null);
                var optionEditText = dialogView.FindViewById<EditText>(Resource.Id.optionEditText);
                var isCorrectCheckBox = dialogView.FindViewById<CheckBox>(Resource.Id.isCorrectCheckBox);

                // Set initial values
                optionEditText.Text = option.Text;
                isCorrectCheckBox.Checked = option.IsCorrect;

                var alertDialog = new AlertDialog.Builder(_activity);
                alertDialog.SetTitle("Edit Option");
                alertDialog.SetView(dialogView);
                alertDialog.SetPositiveButton("Save", (senderAlert, args) => {
                    string optionText = optionEditText.Text?.Trim();
                    if (!string.IsNullOrEmpty(optionText))
                    {
                        // Update option text
                        option.Text = optionText;

                        // If this option is being marked as correct, unmark other options
                        if (isCorrectCheckBox.Checked && !option.IsCorrect)
                        {
                            foreach (var existingOption in question.Options)
                            {
                                existingOption.IsCorrect = (existingOption == option);
                            }
                        }
                        else
                        {
                            option.IsCorrect = isCorrectCheckBox.Checked;
                        }

                        // If no option is marked as correct, show warning
                        if (!question.Options.Any(o => o.IsCorrect))
                        {
                            Toast.MakeText(_activity, "Warning: No correct answer is marked for this question", ToastLength.Long).Show();
                        }

                        // Update UI
                        int questionPosition = _questions.IndexOf(question);
                        if (questionPosition != -1)
                        {
                            NotifyItemChanged(questionPosition);
                        }
                    }
                    else
                    {
                        Toast.MakeText(_activity, "Option text is required", ToastLength.Short).Show();
                    }
                });
                alertDialog.SetNegativeButton("Cancel", (senderAlert, args) => {
                    // Do nothing
                });
                alertDialog.Show();
            }

            private void ShowDeleteOptionConfirmation(QuestionAdmin question, OptionAdmin option)
            {
                // Don't allow deletion if there are only 2 options
                if (question.Options.Count <= 2)
                {
                    Toast.MakeText(_activity, "Questions must have at least 2 options", ToastLength.Short).Show();
                    return;
                }

                var alertDialog = new AlertDialog.Builder(_activity);
                alertDialog.SetTitle("Delete Option");
                alertDialog.SetMessage("Are you sure you want to delete this option?");
                alertDialog.SetPositiveButton("Delete", (senderAlert, args) => {
                    // Check if this was the correct option
                    bool wasCorrect = option.IsCorrect;

                    // Remove option
                    question.Options.Remove(option);

                    // If the deleted option was correct, mark the first remaining option as correct
                    if (wasCorrect && question.Options.Count > 0)
                    {
                        question.Options[0].IsCorrect = true;
                    }

                    // Update UI
                    int questionPosition = _questions.IndexOf(question);
                    if (questionPosition != -1)
                    {
                        NotifyItemChanged(questionPosition);
                    }
                });
                alertDialog.SetNegativeButton("Cancel", (senderAlert, args) => {
                    // Do nothing
                });
                alertDialog.Show();
            }

            // ViewHolder for question items
            private class QuestionViewHolder : RecyclerView.ViewHolder
            {
                public TextView QuestionNumberTextView { get; }
                public TextView QuestionTextView { get; }
                public LinearLayout OptionsContainer { get; }
                public Button AddOptionButton { get; }
                public Button EditButton { get; }
                public Button DeleteButton { get; }

                public QuestionViewHolder(View itemView) : base(itemView)
                {
                    QuestionNumberTextView = itemView.FindViewById<TextView>(Resource.Id.questionNumberTextView);
                    QuestionTextView = itemView.FindViewById<TextView>(Resource.Id.questionTextView);
                    OptionsContainer = itemView.FindViewById<LinearLayout>(Resource.Id.optionsContainer);
                    AddOptionButton = itemView.FindViewById<Button>(Resource.Id.addOptionButton);
                    EditButton = itemView.FindViewById<Button>(Resource.Id.editButton);
                    DeleteButton = itemView.FindViewById<Button>(Resource.Id.deleteButton);
                }
            }
        }
    }
}