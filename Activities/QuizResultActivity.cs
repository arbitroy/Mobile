using Android.App;
using Android.OS;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mobile.Models;
using Mobile.Services;
using Android.Content;
using AndroidX.RecyclerView.Widget;
using Android.Views;
using Android.Graphics;

namespace Mobile.Activities
{
    [Activity(Label = "Quiz Result")]
    public class QuizResultActivity : Activity
    {
        private TextView _quizTitleTextView;
        private TextView _scoreTextView;
        private TextView _timeTextView;
        private RecyclerView _questionsRecyclerView;
        private Button _doneButton;
        
        private ApiService _apiService;
        private int _attemptId;
        private QuizResult _quizResult;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the layout resource
            SetContentView(Resource.Layout.activity_quiz_result);

            // Get attempt ID from intent
            _attemptId = Intent.GetIntExtra("AttemptId", 0);

            // Initialize UI elements
            _quizTitleTextView = FindViewById<TextView>(Resource.Id.quizTitleTextView);
            _scoreTextView = FindViewById<TextView>(Resource.Id.scoreTextView);
            _timeTextView = FindViewById<TextView>(Resource.Id.timeTextView);
            _questionsRecyclerView = FindViewById<RecyclerView>(Resource.Id.questionsRecyclerView);
            _doneButton = FindViewById<Button>(Resource.Id.doneButton);

            // Set up RecyclerView
            _questionsRecyclerView.SetLayoutManager(new LinearLayoutManager(this));

            // Initialize service
            _apiService = new ApiService();
            
            // Load quiz result
            LoadQuizResultAsync();

            // Set up event handlers
            _doneButton.Click += OnDoneButtonClick;
        }

        private async void LoadQuizResultAsync()
        {
            try
            {
                // Show loading indicator
                FindViewById<ProgressBar>(Resource.Id.loadingProgressBar).Visibility = ViewStates.Visible;
                
                // Get quiz result from API
                _quizResult = await _apiService.GetQuizResultAsync(_attemptId);
                
                // Update UI with quiz result
                _quizTitleTextView.Text = _quizResult.QuizTitle;
                _scoreTextView.Text = $"{_quizResult.Score}%";
                
                // Calculate time taken
                TimeSpan timeTaken = _quizResult.EndTime.Value - _quizResult.StartTime;
                _timeTextView.Text = $"{(int)timeTaken.TotalMinutes}:{timeTaken.Seconds:D2}";
                
                // Set up questions RecyclerView
                var adapter = new QuestionResultAdapter(this, _quizResult.Questions);
                _questionsRecyclerView.SetAdapter(adapter);
            }
            catch (Exception ex)
            {
                Toast.MakeText(this, $"Failed to load quiz result: {ex.Message}", ToastLength.Long).Show();
                Finish(); // Close activity
            }
            finally
            {
                // Hide loading indicator
                FindViewById<ProgressBar>(Resource.Id.loadingProgressBar).Visibility = ViewStates.Gone;
            }
        }

        private void OnDoneButtonClick(object sender, EventArgs e)
        {
            // Navigate back to quiz list
            var intent = new Intent(this, typeof(QuizListActivity));
            intent.SetFlags(ActivityFlags.ClearTop); // Clear back stack
            StartActivity(intent);
            Finish();
        }
    }

    // RecyclerView adapter for question results
    public class QuestionResultAdapter : RecyclerView.Adapter
    {
        private readonly Activity _activity;
        private readonly List<QuestionResult> _questions;

        public QuestionResultAdapter(Activity activity, List<QuestionResult> questions)
        {
            _activity = activity;
            _questions = questions;
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            View itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.layout_question_result_item, parent, false);
            return new QuestionResultViewHolder(itemView);
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            QuestionResultViewHolder viewHolder = holder as QuestionResultViewHolder;
            QuestionResult question = _questions[position];
            
            // Set question text and number
            viewHolder.QuestionNumberTextView.Text = $"Question {position + 1}";
            viewHolder.QuestionTextView.Text = question.QuestionText;
            
            // Set selected option
            viewHolder.SelectedOptionTextView.Text = question.SelectedOptionText;
            
            // Set status (correct/incorrect)
            if (question.IsCorrect)
            {
                viewHolder.StatusImageView.SetImageResource(Resource.Drawable.ic_correct);
                viewHolder.SelectedOptionTextView.SetTextColor(Color.ParseColor("#4CAF50")); // Green
            }
            else
            {
                viewHolder.StatusImageView.SetImageResource(Resource.Drawable.ic_incorrect);
                viewHolder.SelectedOptionTextView.SetTextColor(Color.ParseColor("#F44336")); // Red
                
                // Show correct answer
                viewHolder.CorrectOptionLayout.Visibility = ViewStates.Visible;
                viewHolder.CorrectOptionTextView.Text = question.CorrectOptionText;
            }
        }

        public override int ItemCount => _questions.Count;

        // ViewHolder for question result items
        public class QuestionResultViewHolder : RecyclerView.ViewHolder
        {
            public TextView QuestionNumberTextView { get; }
            public TextView QuestionTextView { get; }
            public TextView SelectedOptionTextView { get; }
            public ImageView StatusImageView { get; }
            public LinearLayout CorrectOptionLayout { get; }
            public TextView CorrectOptionTextView { get; }

            public QuestionResultViewHolder(View itemView) : base(itemView)
            {
                QuestionNumberTextView = itemView.FindViewById<TextView>(Resource.Id.questionNumberTextView);
                QuestionTextView = itemView.FindViewById<TextView>(Resource.Id.questionTextView);
                SelectedOptionTextView = itemView.FindViewById<TextView>(Resource.Id.selectedOptionTextView);
                StatusImageView = itemView.FindViewById<ImageView>(Resource.Id.statusImageView);
                CorrectOptionLayout = itemView.FindViewById<LinearLayout>(Resource.Id.correctOptionLayout);
                CorrectOptionTextView = itemView.FindViewById<TextView>(Resource.Id.correctOptionTextView);
            }
        }
    }
}