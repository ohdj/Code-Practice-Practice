package org.mcmodule.mchelper.module;

import org.mcmodule.mchelper.McHelper;
import org.mcmodule.mchelper.event.annotations.EventTarget;
import org.mcmodule.mchelper.event.events.KeyPressEvent;
import org.mcmodule.mchelper.module.render.HUD;

import java.util.Collection;
import java.util.Map;
import java.util.concurrent.ConcurrentHashMap;

public class ModuleManager {
    private final Map<String, Module> moduleMap = new ConcurrentHashMap<>();

    public ModuleManager() {
        registerModule(new HUD());

        McHelper.getInstance().getEventManager().register(this);
    }

    @EventTarget
    public void onKey(KeyPressEvent event) {
        for (Module module : getModules()) {
            if (module.getKey() != 0 && event.getKey() == module.getKey()) {
                module.setEnabled(!module.isEnabled());
            }
        }
    }

    private void registerModule(Module module) {
        moduleMap.put(module.getName(), module);
    }

    public Module getModule(String name) {
        return moduleMap.get(name);
    }

    public Collection<Module> getModules() {
        return moduleMap.values();
    }

    @EventTarget
    public void onKey() {

    }
}
