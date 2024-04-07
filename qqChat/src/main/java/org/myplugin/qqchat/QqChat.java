package org.myplugin.qqchat;

import org.bukkit.Bukkit;
import org.bukkit.Location;
import org.bukkit.configuration.file.FileConfiguration;
import org.bukkit.configuration.file.YamlConfiguration;
import org.bukkit.entity.Entity;
import org.bukkit.entity.EntityType;
import org.bukkit.entity.Player;
import org.bukkit.entity.Villager;
import org.bukkit.event.EventHandler;
import org.bukkit.event.Listener;
import org.bukkit.event.entity.EntityDamageByEntityEvent;
import org.bukkit.event.entity.EntityDeathEvent;
import org.bukkit.event.entity.PlayerDeathEvent;
import org.bukkit.event.player.AsyncPlayerChatEvent;
import org.bukkit.event.player.PlayerJoinEvent;
import org.bukkit.event.player.PlayerQuitEvent;
import org.bukkit.plugin.java.JavaPlugin;
import org.java_websocket.client.WebSocketClient;
import org.java_websocket.handshake.ServerHandshake;

import java.io.File;
import java.net.URI;
import java.net.URISyntaxException;
import java.util.Objects;

// 定义插件主类，继承自JavaPlugin并实现Listener接口
public class QqChat extends JavaPlugin implements Listener {

    private WebSocketClient wsClient; // WebSocket客户端对象
    private String wsUrl; // go-cqhttp的地址和端口，根据实际情况修改
    private long groupId; // qq群号，根据实际情况修改

    // 插件启用时执行的方法
    @Override
    public void onEnable() {
        // 注册事件监听器
        Bukkit.getPluginManager().registerEvents(this, this);

        // 创建 qqChat 文件夹（如果它不存在）
        File dataFolder = getDataFolder();
        if (!dataFolder.exists()) {
            dataFolder.mkdir();
        }

        // 创建指向 qqChat 文件夹中 config.yml 的 File 对象
        File configFile = new File(dataFolder, "config.yml");

        // 如果 config.yml 文件不存在，则从资源中复制默认的 config.yml
        if (!configFile.exists()) {
            saveResource("config.yml", false);
        }

        // 从 config.yml 文件中读取并加载配置数据
        FileConfiguration config = YamlConfiguration.loadConfiguration(configFile);
        groupId = config.getLong("qq-group-id");
        wsUrl = config.getString("ws-url");

        // 创建WebSocket客户端对象，并重写一些方法
        try {
            if (wsUrl != null) {
                wsClient = new WebSocketClient(new URI(wsUrl)) {
                    // 连接成功时执行的方法
                    @Override
                    public void onOpen(ServerHandshake serverHandshake) {
                        getLogger().info("WebSocket连接成功"); // 打印日志信息
                    }

                    // 接收到消息时执行的方法
                    @Override
                    public void onMessage(String s) {
                        // 解析消息内容，判断是否是qq群消息，以及是否包含list命令
                        if (s.contains("\"post_type\":\"message\"") && s.contains("\"group_id\":" + groupId) && s.contains("\"raw_message\":\"list\"")) {
                            // 获取在线玩家列表，并拼接成字符串
                            StringBuilder playerListBuilder = new StringBuilder();
                            int count = 0;
                            for (Player player : Bukkit.getOnlinePlayers()) {
                                playerListBuilder.append(player.getName()).append("\n");
                                count++;
                            }
                            // 如果没有在线玩家，就发送提示信息
                            if (count == 0) {
                                playerListBuilder.append("当前没有在线玩家");
                            }
                            // 调用自定义的方法发送玩家列表到qq群
                            sendToGroup("当前有" + count + "位玩家在线: \n" + playerListBuilder.toString());
                        }
                    }

                    // 连接关闭时执行的方法
                    @Override
                    public void onClose(int i, String s, boolean b) {
                        getLogger().info("WebSocket连接关闭"); // 打印日志信息
                    }

                    // 出现错误时执行的方法
                    @Override
                    public void onError(Exception e) {
                        getLogger().info("WebSocket出现错误"); // 打印日志信息和错误堆栈
                        e.printStackTrace();
                    }
                };
            }
            // 开始连接WebSocket服务器
            wsClient.connect();
        } catch (URISyntaxException e) {
            getLogger().info("WebSocket地址无效"); // 打印日志信息和错误堆栈
            e.printStackTrace();
        }
    }

    // 插件禁用时执行的方法
    @Override
    public void onDisable() {
        // 关闭WebSocket连接
        wsClient.close();
    }

    // 玩家聊天事件处理方法，使用@EventHandler注解标记
    @EventHandler
    public void onPlayerChat(AsyncPlayerChatEvent event) {
        // 获取玩家名称和聊天内容，并拼接成字符串
        String chatMessage = event.getPlayer().getName() + ": " + event.getMessage();
        // 调用自定义的方法发送聊天内容到qq群
        sendToGroup(chatMessage);
    }

    // 玩家进入游戏事件处理方法，使用@EventHandler注解标记
    @EventHandler
    public void onPlayerJoin(PlayerJoinEvent event) {
        // 获取玩家名称，并拼接成字符串
        String joinMessage = event.getPlayer().getName() + "进入了服务器";
        // 调用自定义的方法发送进入信息到qq群
        sendToGroup(joinMessage);
    }

    // 玩家退出游戏事件处理方法，使用@EventHandler注解标记
    @EventHandler
    public void onPlayerQuit(PlayerQuitEvent event) {
        // 获取玩家名称，并拼接成字符串
        String quitMessage = event.getPlayer().getName() + "退出了服务器";
        // 调用自定义的方法发送退出信息到qq群
        sendToGroup(quitMessage);
    }

    // 玩家死亡事件处理方法，使用@EventHandler注解标记
    @EventHandler
    public void onPlayerDeath(PlayerDeathEvent event) {
        // 获取玩家死亡信息，并拼接成字符串
        String deathMessage = event.getDeathMessage();
        // 调用自定义的方法发送死亡信息到qq群
        if (deathMessage != null) {
            sendToGroup(deathMessage);
        }
    }

    // 实体死亡事件处理方法，使用@EventHandler注解标记
    @EventHandler
    public void onEntityDeath(EntityDeathEvent event) {
        // 判断死亡的实体是否是村民，如果是，则继续处理
        if (event.getEntityType() == EntityType.VILLAGER) {
            // 获取村民对象和死亡原因，并拼接成字符串
            Villager villager = (Villager) event.getEntity();
            String deathMessage = "";
            // 判断死因
            if (event.getEntity().getLastDamageCause() instanceof EntityDamageByEntityEvent) {
                // 如果村民是被其他实体杀死的，就执行以下操作
                EntityDamageByEntityEvent damageEvent = (EntityDamageByEntityEvent) event.getEntity().getLastDamageCause(); // 获取最后一次伤害事件对象
                Entity killer = damageEvent.getDamager(); // 获取杀死村民的实体对象

                if (killer instanceof Player) {
                    // 如果杀死村民的实体是玩家，就执行以下操作
                    String killerName = killer.getName(); // 获取玩家姓名
                    deathMessage = "玩家" + killerName + "攻击";
                } else {
                    // 如果杀死村民的实体不是玩家，就执行以下操作
                    String killerType = killer.getType().toString(); // 获取杀死村民的实体类型
                    String killerName = killer.getCustomName(); // 获取杀死村民的实体自定义名字，如果没有就返回null
                    deathMessage = (killerName == null ? killerType + "实体" : killerType + "(命名为" + killerName + ")" + "的实体") + "攻击";
                }
            } else {
                // 如果村民不是被其他实体杀死的，就执行以下操作
                deathMessage = Objects.requireNonNull(event.getEntity().getLastDamageCause()).getCause().toString(); // 获取村民的死亡原因
            }
            // 获取村民的死亡坐标，并拼接成字符串
            Location loc = villager.getLocation();
            String deathLocation = "死亡坐标是: x=" + loc.getBlockX() + ", y=" + loc.getBlockY() + ", z=" + loc.getBlockZ();
            // 调用自定义的方法发送死亡信息和坐标到qq群
            sendToGroup("一个村民因为" + deathMessage + "而死亡\n" + deathLocation);
        }
    }

    // 自定义的方法，用于发送信息到qq群，参数为要发送的信息
    private void sendToGroup(String message) {
        // 发送信息到qq群，注意转义双引号
        wsClient.send("{\"action\":\"send_group_msg\",\"params\":{\"group_id\":" + groupId + ",\"message\":\"" + message.replace("\"", "\\\"") + "\"}}");
    }
}
