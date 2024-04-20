package com.example.calendar.calendarlibrary.main;

import android.app.Activity;
import android.content.Context;
import android.content.Intent;
import android.os.Bundle;

import android.util.Log;
import com.example.calendar.calendarlibrary.calendar.CalendarModel;
import com.example.calendar.calendarlibrary.calendar.CalendarsArrayAdapter;
import com.example.calendar.calendarlibrary.calendar.CalendarsProxy;

import java.util.ArrayList;
import java.util.List;

public class CalendarsActivity
        extends UnityPlayerActivity
        implements CalendarsProxy.Listener {

    private CalendarsProxy mCalendarsQueryHandler;

    private ArrayList<CalendarModel> mCalendars;
    private CalendarsArrayAdapter mCalendarsAdapter;

    public static Context ctx;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        Log.d("OverrideActivity", "onCreate called!");
        super.onCreate(savedInstanceState);

        this.mCalendars = new ArrayList<>();

        this.mCalendarsAdapter = new CalendarsArrayAdapter(this, 0, this.mCalendars);

        this.mCalendarsQueryHandler = new CalendarsProxy(this.getContentResolver());
        this.mCalendarsQueryHandler.registerListener(this);
        CalendarsActivity.ctx = this;
        this.queryCalendars();
    }

    // called from Unity
    public void queryCalendars() {
        Log.d("OverrideActivity", "calendars querried");
        this.mCalendarsQueryHandler.getAll(CalendarModel.create(), null, null);
    }

    @SuppressWarnings("unused")
    public String getAllCalendarNames() {
        String ret = "";
        for (int i = 0; i < this.mCalendars.size(); i++) {
            ret += this.mCalendars.get(i).getDisplayName() + "|";
        }

        return ret;
    }

    @Override
    public void onCalendarCreated() {
        this.queryCalendars();
    }

    @Override
    public void onCalendarDeleted() {
        this.queryCalendars();
    }

    @Override
    public void onCalendarsRetrieved(List<CalendarModel> calendars) {
        this.mCalendars.clear();
        this.mCalendars.addAll(calendars);
        this.mCalendarsAdapter.notifyDataSetChanged();

       // startActivity(new Intent(CalendarsActivity.this, EventsActivity.class)
         //       .putExtra(EventsActivity.EXTRA_CALENDAR_ID, mCalendars.get(0).getId()));
    }
}
