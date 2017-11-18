using System;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using System.Collections.Generic;
using AndroidSwipeListView.Droid.Adapter;
using Java.Lang;

namespace AndroidSwipeListView.Droid
{
    [Activity(Label = "AndroidSwipeListView.Android", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity, View.IOnTouchListener, ViewTreeObserver.IOnPreDrawListener
    {

        private int _position;
        private View _swipeView;
        private static int swipe_threshold = 100;
        private static int swipe_velocity_threshold = 100;
        private float _swipeUp = 0;
        private float _mDownX;
        private int _mSwipeSlop = -1;
        private bool _mSwiping = false;
        private bool _mItemPressed = false;
        private ListView mListView;
        private static int swipe_duration = 250;
        private static int move_duration = 150;
        private CheeseAdapter mAdapter;
        Dictionary<long, int> mItemIdTopMap = new Dictionary<long, int>();
        private List<string> cheeseList;
        private ViewTreeObserver observer;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            // Get our button from the layout resource,
            // and attach an event to it
            mListView = FindViewById<ListView>(Resource.Id.ListView);
            cheeseList = new List<string>();
            for (int i = 0; i < Cheeses.sCheeseStrings.Length; ++i)
            {
                cheeseList.Add(Cheeses.sCheeseStrings[i]);
            }
            mAdapter = new CheeseAdapter(this, Resource.Layout.Row_item, cheeseList,
                    this);
            mListView.Adapter = mAdapter;

        }

        public bool OnTouch(View v, MotionEvent e)
        {
            _swipeView = v;
            _position = mListView.GetPositionForView(v);

            if (_mSwipeSlop < 0)
            {
                _mSwipeSlop = ViewConfiguration.Get(this).ScaledTouchSlop;
            }

            switch (e.Action)
            {
                case MotionEventActions.Down:
                    if (_mItemPressed)
                    {
                        // Multi-item swipes not handled
                        return false;
                    }
                    _mItemPressed = true;
                    _mDownX = e.GetX();
                    //int position = ListView.GetPositionForView(v);
                    View child = mListView.GetChildAt(_position);
                    break;
                case MotionEventActions.Cancel:
                    v.Alpha = 1;
                    v.TranslationX = 0;
                    _mItemPressed = false;
                    break;
                case MotionEventActions.Move:
                    float x = e.GetX() + v.TranslationX;
                    float deltaX = x - _mDownX;
                    float deltaXAbs = Java.Lang.Math.Abs(deltaX);
                    if (!_mSwiping)
                    {
                        if (deltaXAbs > _mSwipeSlop)
                        {
                            _mSwiping = true;
                            mListView.RequestDisallowInterceptTouchEvent(true);
                            // _mBackgroundContainer.ShowBackground(GetRelativePosition(v).Top, v.Height);
                        }
                    }
                    if (_mSwiping)
                    {
                        mListView.RequestDisallowInterceptTouchEvent(true);
                        v.TranslationX = x - _mDownX;
                        v.Alpha = 1 - (deltaXAbs / v.Width);
                    }

                    break;
                case MotionEventActions.Up:
                    {
                        if (_mSwiping)
                        {
                            float xx = e.GetX() + v.TranslationX;
                            float deltaXX = xx - _mDownX;
                            float deltaXXAbs = Java.Lang.Math.Abs(deltaXX);
                            float fractionCovered;
                            float endX;
                            float endAlpha;
                            bool remove = false;
                            mListView.Enabled = false;
                            if (deltaXXAbs > v.Width / 2)
                            {
                                // Greater than a quarter of the width - animate it out
                                fractionCovered = deltaXXAbs / v.Width;
                                endX = deltaXX < 0 ? -v.Width : v.Width;
                                endAlpha = 0;
                                remove = true;
                            }
                            else
                            {
                                // Not far enough - animate it back
                                fractionCovered = 1 - (deltaXXAbs / v.Width);
                                endX = 0;
                                endAlpha = 1;
                                remove = false;
                            }
                            // Animate position and alpha of swiped item
                            // NOTE: This is a simplified version of swipe behavior, for the
                            // purposes of this demo about animation. A real version should use
                            // velocity (via the VelocityTracker class) to send the item off or
                            // back at an appropriate speed.
                            long duration = (int)((1 - fractionCovered) * swipe_duration);


                            v.Animate().SetDuration(duration).
                          Alpha(endAlpha).TranslationX(endX).
                          WithEndAction(new Runnable(() =>
                          {
                                  // Restore animated values
                                  v.Alpha = 1;
                              v.TranslationX = 0;
                              if (remove)
                              {
                                  AnimateRemoval(mListView, v);
                                  mListView.Enabled = true;
                              }
                              else
                              {
                                      //_mBackgroundContainer.HideBackground();
                                      mListView.Enabled = true;
                              }
                          }));
                        }
                        else
                        {
                            // OnItemSelected should be here
                        }
                        _mSwiping = false;
                    }

                    _mItemPressed = false;
                    break;

                default:
                    return false;
            }

            return true;
        }

        private void AnimateRemoval(ListView listview, View viewToRemove)
        {

            if (viewToRemove != null)
            {
                int removePosition = mListView.GetPositionForView(viewToRemove);
                View child = listview.GetChildAt(removePosition);
                long itemId = mAdapter.GetItemId(removePosition);
                mItemIdTopMap.Add(itemId, child.Top);
            }

            // Delete the item from the adapter
            int position = mListView.GetPositionForView(viewToRemove);
            cheeseList.Remove(mAdapter.GetItem(position).ToString());
            mAdapter.NotifyDataSetChanged();


            observer = listview.ViewTreeObserver;
        }



        public bool OnPreDraw()
        {
            observer.RemoveOnPreDrawListener(this);
            bool firstAnimation = true;
            int firstVisiblePosition = mListView.FirstVisiblePosition;
            for (int i = 0; i < mListView.ChildCount; ++i)
            {
                View child = mListView.GetChildAt(i);
                int position = firstVisiblePosition + i;
                long itemId = mAdapter.GetItemId(position);
                int startTop = mItemIdTopMap[itemId];
                int top = child.Top;
                if (startTop != null)
                {
                    if (startTop != top)
                    {
                        int delta = startTop - top;
                        child.TranslationY = delta;
                        child.Animate().SetDuration(move_duration).TranslationY(0);
                        if (firstAnimation)
                        {

                            child.Animate().WithEndAction(new Runnable(() =>
                            {
                                //public void run()
                                //{
                                //    //mBackgroundContainer.hideBackground();
                                _mSwiping = false;
                                mListView.Enabled = true;

                                //}
                            }));
                            firstAnimation = false;

                        }
                    }
                    else
                    {

                        int childHeight = child.Height + mListView.DividerHeight;
                        startTop = top + (i > 0 ? childHeight : -childHeight);
                        int delta = startTop - top;
                        child.TranslationY = delta;
                        child.Animate().SetDuration(move_duration).TranslationY(0);
                        if (firstAnimation)
                        {
                            child.Animate().WithEndAction(new Runnable(() =>
                            {
                                //public void run()
                                //{
                                //mBackgroundContainer.hideBackground();
                                _mSwiping = false;
                                mListView.Enabled = true;
                                //}
                            }));
                            firstAnimation = false;
                        }
                    }

                }
            }
            mItemIdTopMap.Clear();
            return true;
        }
    }
}


