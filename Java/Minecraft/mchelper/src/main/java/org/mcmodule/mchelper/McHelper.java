package org.mcmodule.mchelper;

import net.minecraftforge.common.MinecraftForge;
import net.minecraftforge.fml.common.Mod;
import org.mcmodule.mchelper.event.EventManager;
import org.mcmodule.mchelper.module.ModuleManager;

@Mod("mchelper")
public class McHelper {
    private static McHelper instance;
    private final EventManager eventManager;
    private final ModuleManager moduleManager;

    public McHelper() {
        instance = this;
        eventManager = new EventManager();
        moduleManager = new ModuleManager();
    }

    public static McHelper getInstance() {
        return instance;
    }

    public EventManager getEventManager() {
        return eventManager;
    }
}
