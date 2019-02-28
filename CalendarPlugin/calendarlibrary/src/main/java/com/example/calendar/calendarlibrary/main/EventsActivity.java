package com.example.calendar.calendarlibrary.main;

import android.content.Context;
import android.os.Bundle;
import android.provider.CalendarContract;

import com.example.calendar.calendarlibrary.calendar.CalendarModel;
import com.example.calendar.calendarlibrary.calendar.CalendarsArrayAdapter;
import com.example.calendar.calendarlibrary.calendar.CalendarsProxy;
import com.example.calendar.calendarlibrary.event.EventModel;
import com.example.calendar.calendarlibrary.event.EventsProxy;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

public class EventsActivity
        extends UnityPlayerActivity
        implements EventsProxy.Listener, CalendarsProxy.Listener {

    private EventsProxy mEventsQueryHandler;

    private Map<Integer, List<EventModel>> allCalendarsAndEvents;

    private CalendarsProxy mCalendarsQueryHandler;

    private ArrayList<CalendarModel> mCalendars;
    private CalendarsArrayAdapter mCalendarsAdapter;

    // TO ASSEMBLE THE .aar -> under Gradle got to calendarLibrary -> Tasks -> build -> double click "assemble"
    // THEN COPY CalendarPlugin\calendarlibrary\build\outputs\aar\calendarlibrary-debug.aar ->
    // SUNDIAL\Assets\Plugins\Android

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);

        this.allCalendarsAndEvents = new HashMap<>();

        this.mEventsQueryHandler = new EventsProxy(this.getContentResolver());
        this.mEventsQueryHandler.registerListener(this);
        
        queryEvents();

        this.mCalendars = new ArrayList<>();

        this.mCalendarsAdapter = new CalendarsArrayAdapter(this, 0, this.mCalendars);

        this.mCalendarsQueryHandler = new CalendarsProxy(this.getContentResolver());
        this.mCalendarsQueryHandler.registerListener(this);
        this.queryCalendars();
    }

    public void queryEvents() {
        for (int i = 1; i < 50; i++) {
            this.mEventsQueryHandler.getAll(
                    EventModel.create(),
                    CalendarContract.Events.CALENDAR_ID + " = ?",
                    new String[]{String.valueOf(i)});
        }
    }

    public void queryCalendars() {
        this.mCalendarsQueryHandler.getAll(CalendarModel.create(), null, null);
    }

    @SuppressWarnings("unused")
    public byte[] getAllCalendarNames() {
        String ret = "";
        for (int i = 0; i < this.mCalendars.size(); i++) {
            ret += this.mCalendars.get(i).getId() + ":" + this.mCalendars.get(i).getDisplayName() + "|";
        }

        return ret.getBytes();
    }

    @SuppressWarnings("unused")
    public byte[] getAllEventsTitles(int index) {
        String eventString = "";
        List<EventModel> events = this.allCalendarsAndEvents.get(index);
        if(events == null) return new byte[0];
        for (int i = 0; i < events.size(); i++) {
            EventModel event = events.get(i);
            if (event.getTitle() == null || event.getTitle() == "") {
                eventString += "0|";
            } else {
                eventString += event.getTitle() + "|";
            }
        }

        return eventString.getBytes();
    }

    @SuppressWarnings("unused")
    public int[] getAllEventsStartEnd(int index) {
        List<EventModel> events = this.allCalendarsAndEvents.get(index);
        if(events == null) return new int[0];
        int[] ret = new int[events.size() * 2];
        int counter = 0;

        for (int i = 0; i < events.size(); i++) {
            EventModel event = events.get(i);

            // these are unsafe long to int casts -> shorten them by 10000 and then re-inflate
            ret[counter] = (int) (event.getStartDate().getTime().getTime() * .0001);
            ret[counter + 1] = (int) (event.getEndDate().getTime().getTime() * .0001);
            counter += 2;
        }
        return ret;
    }

    @Override
    public void onEventsRetrieved(List<EventModel> events) {
        if (events.size() > 0) {
            this.allCalendarsAndEvents.put(events.get(0).getCalendarId(), events);
        }
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

    @Override
    public void onCalendarsRetrieved(List<CalendarModel> calendars) {
        this.mCalendars.clear();
        this.mCalendars.addAll(calendars);
        this.mCalendarsAdapter.notifyDataSetChanged();
    }

    @Override
    public void onCalendarCreated() {

    }

    @Override
    public void onCalendarDeleted() {

    }
}
