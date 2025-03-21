using Android.App;
using Android.OS;
using Android.Widget;
using System;
using System.Collections.Generic;
using Mobile.Models;
using Mobile.Services;
using Android.Content;
using AndroidX.RecyclerView.Widget;
using Android.Views;
using System.Linq;

namespace Mobile.Activities
{
    [Activity(Label = "Quiz History")]
    public class QuizHistoryActivity : BaseAuthenticatedActivity
    {
        private RecyclerView _historyRecyclerView;
        private ProgressBar _loadingProgressBar;
        private TextView _emptyTextView;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the layout resource
            SetContentView(Resource.Layout.activity_quiz_history);

            // Initialize UI elements
            _historyRecyclerView = FindViewById<RecyclerView>(Resource.Id.historyRecyclerView);
            _loadingProgressBar = FindViewById<ProgressBar>(Resource.Id.loadingProgressBar);
            _emptyTextView = FindViewById<TextView>(Resource.Id.emptyTextView);

            // Set up RecyclerView
            _historyRecyclerView.SetLayoutManager(new LinearLayoutManager(this));

            // Load quiz history
            LoadHistoryAsync();
        }

        private async void LoadHistoryAsync()
        {
            try
            {
                // Show loading indicator
                _loadingProgressBar.Visibility = Android.Views.ViewStates.Visible;

                // Get history from API
                var historyItems = await ApiService.GetUserHistoryAsync();

                if (historyItems.Count > 0)
                {
                    // Set up history RecyclerView
                    var adapter = new HistoryAdapter(this, historyItems);
                    _historyRecyclerView.SetAdapter(adapter);

                    // Hide empty message
                    _emptyTextView.Visibility = Android.Views.ViewStates.Gone;
                }
                else
                {
                    // Show empty message
                    _emptyTextView.Visibility = Android.Views.ViewStates.Visible;
                }
            }
            catch (UnauthorizedAccessException)
            {
                HandleAuthError();
            }
            catch (Exception ex)
            {
                Toast.MakeText(this, $"Failed to load history: {ex.Message}", ToastLength.Long).Show();
                // Show empty message
                _emptyTextView.Visibility = Android.Views.ViewStates.Visible;
            }
            finally
            {
                // Hide loading indicator
                _loadingProgressBar.Visibility = Android.Views.ViewStates.Gone;
            }
        }

        private void HandleAuthError()
        {
            TokenManager.ClearToken(this);
            Toast.MakeText(this, "Your session has expired. Please log in again.", ToastLength.Long).Show();
            RedirectToLogin();
        }

        // RecyclerView adapter for quiz history
        public class HistoryAdapter : RecyclerView.Adapter
        {
            private readonly Activity _activity;
            private readonly List<QuizAttemptDetailDto> _historyItems;

            public HistoryAdapter(Activity activity, List<QuizAttemptDetailDto> historyItems)
            {
                _activity = activity;
                _historyItems = historyItems;
            }

            public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
            {
                View itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.layout_history_item, parent, false);
                return new HistoryViewHolder(itemView);
            }

            public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
            {
                HistoryViewHolder viewHolder = holder as HistoryViewHolder;
                QuizAttemptDetailDto item = _historyItems[position];

                // Set quiz title
                viewHolder.QuizTitleTextView.Text = item.QuizTitle;

                // Set date
                viewHolder.DateTextView.Text = item.EndTime.ToString("MMM dd, yyyy HH:mm");

                // Set duration
                viewHolder.DurationTextView.Text = $"{item.Duration} min";

                // Set score
                viewHolder.ScoreTextView.Text = $"{item.Score}%";

                // Set score color based on value
                if (item.Score >= 80)
                {
                    viewHolder.ScoreTextView.SetTextColor(Android.Graphics.Color.ParseColor("#4CAF50")); // Green
                }
                else if (item.Score >= 60)
                {
                    viewHolder.ScoreTextView.SetTextColor(Android.Graphics.Color.ParseColor("#FF9800")); // Orange
                }
                else
                {
                    viewHolder.ScoreTextView.SetTextColor(Android.Graphics.Color.ParseColor("#F44336")); // Red
                }

                // Set up event handler for the View Details button
                viewHolder.ViewDetailsButton.Click += (sender, e) => {
                    // Navigate to quiz result activity
                    var intent = new Intent(_activity, typeof(QuizResultActivity));
                    intent.PutExtra("AttemptId", item.AttemptId);
                    _activity.StartActivity(intent);
                };
            }

            public override int ItemCount => _historyItems.Count;

            // ViewHolder for history items
            public class HistoryViewHolder : RecyclerView.ViewHolder
            {
                public TextView QuizTitleTextView { get; }
                public TextView DateTextView { get; }
                public TextView DurationTextView { get; }
                public TextView ScoreTextView { get; }
                public Button ViewDetailsButton { get; }

                public HistoryViewHolder(View itemView) : base(itemView)
                {
                    QuizTitleTextView = itemView.FindViewById<TextView>(Resource.Id.quizTitleTextView);
                    DateTextView = itemView.FindViewById<TextView>(Resource.Id.dateTextView);
                    DurationTextView = itemView.FindViewById<TextView>(Resource.Id.durationTextView);
                    ScoreTextView = itemView.FindViewById<TextView>(Resource.Id.scoreTextView);
                    ViewDetailsButton = itemView.FindViewById<Button>(Resource.Id.viewDetailsButton);
                }
            }
        }
    }
}