package org.mcmodule.mchelper.event.events;

import org.mcmodule.mchelper.event.impl.Event;

public class KeyPressEvent implements Event {
    private final int key;

    public KeyPressEvent(int key) {
        this.key = key;
    }

    public int getKey() {
        return key;
    }
}
