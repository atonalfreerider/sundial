package com.example.calendar.calendarlibrary.main;

import android.app.Activity;
import android.content.Context;
import android.os.Bundle;
import android.provider.CalendarContract;
import android.util.Log;

import com.example.calendar.calendarlibrary.event.EventModel;
import com.example.calendar.calendarlibrary.event.EventsProxy;

import java.util.ArrayList;
import java.util.List;

public class EventsActivity
        extends UnityPlayerActivity
        implements EventsProxy.Listener {

    public static final String EXTRA_CALENDAR_ID = "calendar_id";

    private EventsProxy mEventsQueryHandler;

    private ArrayList<EventModel> mEvents;

    private int mCalendarId;

    public static Context ctx;

    // TO ASSEMBLE THE .aar -> under Gradle got to calendarLibrary -> Tasks -> build -> double click "assemble"
    // THEN COPY CalendarPlugin\calendarlibrary\build\outputs\aar\calendarlibrary-debug.aar ->
    // SUNDIAL\Assets\Plugins\Android

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);

        //this.mCalendarId = this.getIntent().getExtras().getInt(EXTRA_CALENDAR_ID, -1);

        this.mEvents = new ArrayList<>();

        this.mEventsQueryHandler = new EventsProxy(this.getContentResolver());
        this.mEventsQueryHandler.registerListener(this);
        EventsActivity.ctx = this;
        queryEvents(3); // johnbvoorhees calendar (3, 12 and 13 at the time of writing!)
    }

    public void queryEvents(int calendarId) {
        this.mCalendarId = calendarId;
        Log.e("CALENDARS_QUERY", "starting the query");

        this.mEventsQueryHandler.getAll(
                EventModel.create(),
                CalendarContract.Events.CALENDAR_ID + " = ?",
                new String[]{String.valueOf(this.mCalendarId)});
    }

    @SuppressWarnings("unused")
    public byte[] getAllEventsTitles(){
        String eventString = "";
        for(int i = 0; i < this.mEvents.size(); i++){
            EventModel event = this.mEvents.get(i);
            if(event.getTitle() == null || event.getTitle() == ""){
                eventString += "0|";
            }
            else{
                eventString += event.getTitle() + "|";
            }
        }

        return eventString.getBytes();
    }

    @SuppressWarnings("unused")
    public int[] getAllEventsStartEnd(){
        int[] ret = new int[this.mEvents.size() * 2];
        int counter = 0;
        for(int i = 0; i < this.mEvents.size(); i++){
            EventModel event = this.mEvents.get(i);

            // these are unsafe long to int casts
            ret[counter] = (int) event.getStartDate().getTime().getTime();
            ret[counter + 1] = (int) event.getEndDate().getTime().getTime();
            counter += 2;
        }
        return ret;
    }

    @Override
    public void onEventsRetrieved(List<EventModel> events) {
        this.mEvents.clear();
        this.mEvents.addAll(events);
    }

    @Override
    public void onEventCreated() {

    }

    @Override
    public void onEventDeleted() {

    }

    @Override
    public void onEventUpdated() {

    }
}
