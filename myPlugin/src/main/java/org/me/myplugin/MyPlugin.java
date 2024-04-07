package org.me.myplugin;

import org.bukkit.Bukkit;
import org.bukkit.plugin.java.JavaPlugin;
import org.me.myplugin.event.PlayerJoinEventListener;

import java.util.logging.Logger;

public final class MyPlugin extends JavaPlugin {

    @Override
    public void onEnable() {
        // Plugin startup logic
        Logger logger = this.getLogger();
        logger.info("Hello, this is myPlugin");
        logger.warning("这是一条警告");

        // this.getServer().getPluginManager().registerEvents(new PlayerJoinEventListener(), this);
        Bukkit.getPluginManager().registerEvents(new PlayerJoinEventListener(), this);
    }

    @Override
    public void onDisable() {
        // Plugin shutdown logic
    }
}
