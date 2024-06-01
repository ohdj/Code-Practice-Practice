package org.me.myplugin.event;

import org.bukkit.*;
import org.bukkit.entity.Entity;
import org.bukkit.entity.Player;
import org.bukkit.entity.Sheep;
import org.bukkit.event.EventHandler;
import org.bukkit.event.Listener;
import org.bukkit.event.entity.EntityDamageByEntityEvent;
import org.bukkit.event.player.PlayerJoinEvent;

import java.util.Random;

public class PlayerJoinEventListener implements Listener {

    /**
     * 玩家加入服务器 事件处理
     *
     * @param event
     */
    @EventHandler
    public void onPlayerJoin(PlayerJoinEvent event) {
        Player player = event.getPlayer();
        // 当玩家进入服务器时，会在玩家身边播放音效
        Location location = player.getLocation();
        World world = location.getWorld();
        if (world != null) {
            world.playSound(location, Sound.ENTITY_PLAYER_LEVELUP, .5F, .5F);
        }

        // 并且修改玩家进入服务器时全局广播提示
        event.setJoinMessage(ChatColor.GREEN + "让我们欢迎玩家" + player.getName() + "加入了服务器，热烈欢迎~");
    }

    /**
     * 羊毛色彩变化 事件处理
     * 当玩家想要伤害羊羊君的时候羊毛色彩就会发生变化
     */
    @EventHandler
    public void onPlayerHitSheep(EntityDamageByEntityEvent event) {
        Entity damager = event.getDamager();
        if (damager != null && damager instanceof Player) {
            // 施害者是玩家类型
            if (event.getEntity() != null && event.getEntity() instanceof Sheep) {
                // 被害者是羊羊君
                Entity entity = event.getEntity();
                Sheep sheep = (Sheep) entity;

                DyeColor[] colors = DyeColor.values();
                int randomIndex = new Random().nextInt(colors.length);

                // 修改羊毛颜色，随机
                sheep.setColor(colors[randomIndex]);

                // 取消伤害事件发生
                event.setCancelled(true);
            }
        }

    }
}
