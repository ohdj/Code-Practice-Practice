package org.mcmodule.mchelper.module;

import net.minecraft.client.Minecraft;
import org.mcmodule.mchelper.McHelper;

public abstract class Module {
    protected static Minecraft mc = Minecraft.getInstance();
    private final String name;
    private final Category category;
    private int key;

    private boolean enabled;

    protected Module(String name, Category category, int key) {
        this.name = name;
        this.category = category;
        this.key = key;
    }

    public void setEnabled(boolean enabled) {
        this.enabled = enabled;

        if (enabled) {
            McHelper.getInstance().getEventManager().register(this);
            onEnable();
        } else {
            McHelper.getInstance().getEventManager().unregister(this);
            onDisable();
        }
    }

    protected boolean isEnabled(){
        return enabled;
    }

    protected void onEnable() {
    }

    protected void onDisable() {
    }

    public String getName() {
        return name;
    }

    public Category getCategory() {
        return category;
    }

    public int getKey() {
        return key;
    }
}
