using Android.App;
using Android.OS;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mobile.Models;
using Mobile.Services;
using Android.Content;
using Android.Views;
using Android.Graphics;
using AndroidX.ViewPager2.Widget;
using AndroidX.RecyclerView.Widget;
using AndroidX.ViewPager.Widget;

namespace Mobile.Activities
{
    [Activity(Label = "Take Quiz")]
    public class TakeQuizActivity : Activity
    {
        private TextView _quizTitleTextView;
        private TextView _questionCounterTextView;
        private TextView _timerTextView;
        private ProgressBar _progressBar;
        private ViewPager2 _questionViewPager;
        private Button _previousButton;
        private Button _nextButton;

        private QuestionAdapter _questionAdapter;
        private ApiService _apiService;
        private int _quizId;
        private string _quizTitle;
        private QuizDetail _quizDetail;
        private Dictionary<int, int> _answers = new Dictionary<int, int>();

        private System.Timers.Timer _timer;
        private int _timeLeft;
        private DateTime _startTime;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the layout resource
            SetContentView(Resource.Layout.activity_take_quiz);

            // Get quiz ID and title from intent
            _quizId = Intent.GetIntExtra("QuizId", 0);
            _quizTitle = Intent.GetStringExtra("QuizTitle");

            // Initialize UI elements
            _quizTitleTextView = FindViewById<TextView>(Resource.Id.quizTitleTextView);
            _questionCounterTextView = FindViewById<TextView>(Resource.Id.questionCounterTextView);
            _timerTextView = FindViewById<TextView>(Resource.Id.timerTextView);
            _progressBar = FindViewById<ProgressBar>(Resource.Id.progressBar);
            _questionViewPager = FindViewById<ViewPager2>(Resource.Id.questionViewPager);
            _previousButton = FindViewById<Button>(Resource.Id.previousButton);
            _nextButton = FindViewById<Button>(Resource.Id.nextButton);

            // Set quiz title
            _quizTitleTextView.Text = _quizTitle;

            // Initialize service with context to access token
            _apiService = new ApiService(this);

            // Load quiz details
            LoadQuizDetailAsync();

            // Set up event handlers
            _previousButton.Click += OnPreviousButtonClick;
            _nextButton.Click += OnNextButtonClick;

            // Set up ViewPager page change callback
            _questionViewPager.RegisterOnPageChangeCallback(new PageChangeCallback(this));
        }

        // Page change callback class
        private class PageChangeCallback : ViewPager2.OnPageChangeCallback
        {
            private readonly TakeQuizActivity _activity;

            public PageChangeCallback(TakeQuizActivity activity)
            {
                _activity = activity;
            }

            public override void OnPageSelected(int position)
            {
                base.OnPageSelected(position);
                _activity.UpdateQuestionCounter(position);
                _activity.UpdateNavigationButtons(position);
                _activity._progressBar.Progress = position + 1;
            }
        }

        private async void LoadQuizDetailAsync()
        {
            try
            {
                // Show loading overlay or disable UI

                // Get quiz detail from API
                _quizDetail = await _apiService.GetQuizDetailAsync(_quizId);

                // Set quiz title
                _quizTitleTextView.Text = _quizDetail.Title;

                // Set up ViewPager adapter
                _questionAdapter = new QuestionAdapter(this, _quizDetail.Questions);
                _questionViewPager.Adapter = _questionAdapter;

                // Update question counter
                UpdateQuestionCounter(0);

                // Set up progress bar
                _progressBar.Max = _quizDetail.Questions.Count;
                _progressBar.Progress = 1;

                // Start timer
                StartTimer(_quizDetail.TimeLimit);

                // Initialize answers dictionary
                foreach (var question in _quizDetail.Questions)
                {
                    _answers[question.Id] = -1; // -1 means unanswered
                }

                // Update navigation buttons
                UpdateNavigationButtons(0);
            }
            catch (Exception ex)
            {
                // Check if this is an authentication error
                if (ex.Message.Contains("must be logged in") || ex.Message.Contains("Unauthorized"))
                {
                    // Clear invalid token and redirect to login
                    TokenManager.ClearToken(this);
                    Toast.MakeText(this, "Your session has expired. Please log in again.", ToastLength.Long).Show();

                    var intent = new Intent(this, typeof(MainActivity));
                    intent.SetFlags(ActivityFlags.ClearTask | ActivityFlags.NewTask);
                    StartActivity(intent);
                    Finish();
                    return;
                }

                Toast.MakeText(this, $"Failed to load quiz: {ex.Message}", ToastLength.Long).Show();
                Finish(); // Close activity
            }
        }

        private void StartTimer(int minutes)
        {
            _timeLeft = minutes * 60; // Convert minutes to seconds
            _startTime = DateTime.Now;

            _timer = new System.Timers.Timer();
            _timer.Interval = 1000; // 1 second
            _timer.Elapsed += OnTimerElapsed;
            _timer.Start();

            // Initialize timer text
            UpdateTimerText();
        }

        private void OnTimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            _timeLeft--;

            if (_timeLeft <= 0)
            {
                _timer.Stop();
                RunOnUiThread(() => {
                    Toast.MakeText(this, "Time's up! Submitting your answers...", ToastLength.Long).Show();
                    SubmitQuiz();
                });
            }
            else
            {
                RunOnUiThread(UpdateTimerText);
            }
        }

        private void UpdateTimerText()
        {
            int minutes = _timeLeft / 60;
            int seconds = _timeLeft % 60;

            _timerTextView.Text = $"{minutes:D2}:{seconds:D2}";

            // Change color to red when time is running low
            if (_timeLeft <= 60) // Last minute
            {
                _timerTextView.SetTextColor(Color.Red);
            }
        }

        private void UpdateQuestionCounter(int position)
        {
            _questionCounterTextView.Text = $"Question {position + 1} of {_quizDetail.Questions.Count}";
        }

        private void UpdateNavigationButtons(int position)
        {
            // Update Previous button
            _previousButton.Enabled = position > 0;

            // Update Next button
            bool isLastQuestion = position == _quizDetail.Questions.Count - 1;
            _nextButton.Text = isLastQuestion ? "Submit" : "Next";
        }

        private void OnPreviousButtonClick(object sender, EventArgs e)
        {
            int currentPosition = _questionViewPager.CurrentItem;
            if (currentPosition > 0)
            {
                _questionViewPager.CurrentItem = currentPosition - 1;
            }
        }

        private void OnNextButtonClick(object sender, EventArgs e)
        {
            int currentPosition = _questionViewPager.CurrentItem;

            if (currentPosition < _quizDetail.Questions.Count - 1)
            {
                // Go to next question
                _questionViewPager.CurrentItem = currentPosition + 1;
            }
            else
            {
                // Submit quiz (last question)
                ShowSubmitConfirmation();
            }
        }

        private void ShowSubmitConfirmation()
        {
            int unansweredCount = _answers.Count(a => a.Value == -1);

            AlertDialog.Builder builder = new AlertDialog.Builder(this);
            builder.SetTitle("Submit Quiz");

            if (unansweredCount > 0)
            {
                builder.SetMessage($"You have {unansweredCount} unanswered question(s). Are you sure you want to submit?");
            }
            else
            {
                builder.SetMessage("Are you sure you want to submit your answers?");
            }

            builder.SetPositiveButton("Submit", (sender, e) => {
                SubmitQuiz();
            });

            builder.SetNegativeButton("Cancel", (sender, e) => {
                // Do nothing
            });

            builder.Show();
        }

        private async void SubmitQuiz()
        {
            try
            {
                // Stop timer
                _timer?.Stop();

                // Show progress dialog
                var progressDialog = new ProgressDialog(this);
                progressDialog.SetMessage("Submitting your answers...");
                progressDialog.SetCancelable(false);
                progressDialog.Show();

                try
                {
                    // Check if we have answers to submit
                    if (_answers.Count(a => a.Value != -1) == 0)
                    {
                        // No answers selected at all
                        Toast.MakeText(this, "Please select at least one answer before submitting", ToastLength.Long).Show();
                        return;
                    }

                    // Filter out unanswered questions (-1 values)
                    Dictionary<int, int> answersToSubmit = new Dictionary<int, int>();
                    foreach (var pair in _answers)
                    {
                        if (pair.Value != -1)
                        {
                            answersToSubmit.Add(pair.Key, pair.Value);
                        }
                    }

                    // Create submission object
                    var submission = new QuizSubmission
                    {
                        QuizId = _quizId,
                        Answers = answersToSubmit
                    };

                    // Log for debugging
                    Console.WriteLine($"Submitting quiz with {answersToSubmit.Count} answers");
                    foreach (var answer in answersToSubmit)
                    {
                        Console.WriteLine($"Question ID: {answer.Key}, Selected Option ID: {answer.Value}");
                    }

                    // Submit to API
                    var result = await _apiService.SubmitQuizAsync(submission);

                    // Navigate to result activity
                    var intent = new Intent(this, typeof(QuizResultActivity));
                    intent.PutExtra("AttemptId", result.AttemptId);
                    StartActivity(intent);
                    Finish(); // Close quiz activity
                }
                finally
                {
                    if (progressDialog.IsShowing)
                    {
                        progressDialog.Dismiss();
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                // Handle authentication errors specifically
                TokenManager.ClearToken(this);
                Toast.MakeText(this, "Your session has expired. Please log in again.", ToastLength.Long).Show();

                var intent = new Intent(this, typeof(MainActivity));
                intent.SetFlags(ActivityFlags.ClearTask | ActivityFlags.NewTask);
                StartActivity(intent);
                Finish();
            }
            catch (Exception ex)
            {
                // Check if this message indicates an authentication issue
                if (ex.Message.Contains("must be logged in") ||
                    ex.Message.Contains("Unauthorized") ||
                    ex.Message.Contains("session") ||
                    ex.Message.Contains("401") ||
                    ex.Message.Contains("403") ||
                    ex.Message.Contains("login"))
                {
                    // Clear invalid token and redirect to login
                    TokenManager.ClearToken(this);
                    Toast.MakeText(this, "Your session has expired. Please log in again.", ToastLength.Long).Show();

                    var intent = new Intent(this, typeof(MainActivity));
                    intent.SetFlags(ActivityFlags.ClearTask | ActivityFlags.NewTask);
                    StartActivity(intent);
                    Finish();
                    return;
                }

                // Log the full exception details
                Console.WriteLine($"Submit Quiz Exception: {ex}");
                Toast.MakeText(this, $"Failed to submit quiz: {ex.Message}", ToastLength.Long).Show();
            }
        }

        public void SetAnswer(int questionId, int optionId)
        {
            _answers[questionId] = optionId;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _timer?.Stop();
            _timer?.Dispose();
        }
    }

    // RecyclerView adapter for questions in ViewPager2
    public class QuestionAdapter : RecyclerView.Adapter
    {
        private readonly Activity _activity;
        private readonly List<Question> _questions;
        private readonly TakeQuizActivity _quizActivity;

        public QuestionAdapter(Activity activity, List<Question> questions)
        {
            _activity = activity;
            _questions = questions;
            _quizActivity = activity as TakeQuizActivity;
        }

        public override int ItemCount => _questions.Count;

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            View itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.layout_question_item, parent, false);
            return new QuestionViewHolder(itemView);
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            QuestionViewHolder viewHolder = holder as QuestionViewHolder;
            Question question = _questions[position];

            // Set question text
            viewHolder.QuestionTextView.Text = question.Text;

            // Clear previous radio buttons (if recycled)
            viewHolder.OptionsRadioGroup.RemoveAllViews();

            // Add options as radio buttons
            foreach (var option in question.Options)
            {
                RadioButton radioButton = new RadioButton(_activity)
                {
                    Text = option.Text,
                    Tag = option.Id
                };

                // Set layout parameters
                radioButton.LayoutParameters = new RadioGroup.LayoutParams(
                    RadioGroup.LayoutParams.MatchParent,
                    RadioGroup.LayoutParams.WrapContent
                );

                // Add margins to radio button
                var layoutParams = (RadioGroup.LayoutParams)radioButton.LayoutParameters;
                layoutParams.SetMargins(0, 0, 0, 30); // left, top, right, bottom
                radioButton.LayoutParameters = layoutParams;

                // Add to radio group
                viewHolder.OptionsRadioGroup.AddView(radioButton);

                // Set up event handler for option selection
                radioButton.CheckedChange += (sender, e) => {
                    if (e.IsChecked)
                    {
                        int optionId = (int)((RadioButton)sender).Tag;
                        _quizActivity.SetAnswer(question.Id, optionId);
                    }
                };
            }
        }

        // ViewHolder for question items
        public class QuestionViewHolder : RecyclerView.ViewHolder
        {
            public TextView QuestionTextView { get; }
            public RadioGroup OptionsRadioGroup { get; }

            public QuestionViewHolder(View itemView) : base(itemView)
            {
                QuestionTextView = itemView.FindViewById<TextView>(Resource.Id.questionTextView);
                OptionsRadioGroup = itemView.FindViewById<RadioGroup>(Resource.Id.optionsRadioGroup);
            }
        }
    }
}