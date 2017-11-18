using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Java.Lang;

namespace AndroidSwipeListView.Droid.Adapter
{
    public class CheeseAdapter : BaseAdapter
    {
        Dictionary<string, int> mIdMap = new Dictionary<string, int>();
        View.IOnTouchListener mTouchListener;
        private int rowLayout;
        private Context mContext;
        private List<string> mList;

        public CheeseAdapter(Context context, int resId, List<string> list, View.IOnTouchListener listener)
        {
            mTouchListener = listener;
            rowLayout = resId;
            mContext = context;
            mList = list;
            for (int i = 0; i < list.Count(); ++i)
            {
                mIdMap.Add(list[i], i);
            }
        }

        public override long GetItemId(int position)
        {
            string item = mList[position];
            return mIdMap[item];
        }

        public override bool HasStableIds
        {
            get
            {
                return base.HasStableIds;
            }
        }

        public override int Count
        {
            get
            {
              return  mIdMap.Count;
            }
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            View view = convertView ?? ((Activity)mContext).LayoutInflater.Inflate(
        Resource.Layout.Row_item, parent, false);
            if (view != convertView)
            {
                // Add touch listener to every new view to track swipe motion
                view.SetOnTouchListener(mTouchListener);
            }

            TextView cheese = view.FindViewById<TextView> (Resource.Id.txtCheese);
            cheese.Text = mList[position];
            return view;
        }

        public override Java.Lang.Object GetItem(int position)
        {
            return mList[position];
        }
    }
}