#!/usr/bin/env python3
"""
Suíte de testes de unidade e validação lógica do Vizzor.
Verifica os geradores de pacotes de protocolo, regras de clamping, modos de exibição e cálculo de telemetria.
"""

import unittest
import math
from datetime import datetime

class MockHardwareMetrics:
    def __init__(self, cpu_temp=48.4, gpu_temp=42.0, ssd_temp=38.0,
                 cpu_load=22.0, gpu_load=12.0, ram_load=48.1, mb_temp=34.0,
                 cpu_name="AMD Ryzen 5 5600X", gpu_name="NVIDIA GeForce RTX 5060 Ti",
                 cpu_clock_ghz=4.2, cpu_power_w=65.0, gpu_power_w=140.0,
                 gpu_vram_used_gb=4.5, gpu_vram_total_gb=16.0,
                 ram_used_gb=15.4, ram_total_gb=32.0,
                 cpu_fan_rpm=1250, gpu_fan_rpm=0):
        self.cpu_temp = cpu_temp
        self.gpu_temp = gpu_temp
        self.ssd_temp = ssd_temp
        self.cpu_load = cpu_load
        self.gpu_load = gpu_load
        self.ram_load = ram_load
        self.mb_temp = mb_temp
        self.cpu_name = cpu_name
        self.gpu_name = gpu_name
        self.cpu_clock_ghz = cpu_clock_ghz
        self.cpu_power_w = cpu_power_w
        self.gpu_power_w = gpu_power_w
        self.gpu_vram_used_gb = gpu_vram_used_gb
        self.gpu_vram_total_gb = gpu_vram_total_gb
        self.ram_used_gb = ram_used_gb
        self.ram_total_gb = ram_total_gb
        self.cpu_fan_rpm = cpu_fan_rpm
        self.gpu_fan_rpm = gpu_fan_rpm


def clamp(val, min_val=0, max_val=99):
    return max(min_val, min(max_val, int(round(val))))


# Protocol Packet Builders (Replicando as implementações C# para teste formal)
def build_rise_mode_packet(display_val, metrics):
    buf = bytearray(11)
    buf[0] = 0x00
    buf[1] = clamp(display_val, 0, 99)
    buf[2] = clamp(metrics.gpu_temp, 0, 99)
    buf[3] = clamp(metrics.cpu_load, 0, 99)
    buf[4] = clamp(metrics.ram_load, 0, 99)
    buf[5] = clamp(metrics.mb_temp, 0, 99)
    buf[6] = clamp(metrics.gpu_load, 0, 99)
    return bytes(buf)


def build_deepcool_packet(display_val, metrics):
    buf = bytearray(64)
    buf[0] = 0x00
    buf[1] = 0xDA
    buf[2] = clamp(display_val, 0, 99)
    buf[3] = clamp(metrics.cpu_load, 0, 100)
    buf[4] = 0x01
    return bytes(buf)


def build_nzxt_kraken_packet(display_val, metrics):
    buf = bytearray(64)
    buf[0] = 0x00
    buf[1] = 0x70
    buf[2] = 0x01
    buf[3] = clamp(display_val, 0, 99)
    buf[4] = clamp(metrics.gpu_temp, 0, 100)
    return bytes(buf)


def build_lian_li_packet(display_val, metrics):
    buf = bytearray(64)
    buf[0] = 0x00
    buf[1] = 0xE0
    buf[2] = clamp(display_val, 0, 99)
    buf[3] = clamp(metrics.gpu_temp, 0, 100)
    return bytes(buf)


def build_cooler_master_packet(display_val, metrics):
    buf = bytearray(64)
    buf[0] = 0x00
    buf[1] = 0x55
    buf[2] = clamp(display_val, 0, 99)
    buf[3] = clamp(metrics.cpu_load, 0, 100)
    return bytes(buf)


class TestVizzorProtocols(unittest.TestCase):
    def setUp(self):
        self.metrics = MockHardwareMetrics()

    def test_rise_mode_packet_structure(self):
        packet = build_rise_mode_packet(46, self.metrics)
        self.assertEqual(len(packet), 11)
        self.assertEqual(packet[0], 0x00) # Report ID
        self.assertEqual(packet[1], 46)   # Display value
        self.assertEqual(packet[2], 42)   # GPU Temp
        self.assertEqual(packet[3], 22)   # CPU Load
        self.assertEqual(packet[4], 48)   # RAM Load
        self.assertEqual(packet[5], 34)   # MB Temp
        self.assertEqual(packet[6], 12)   # GPU Load
        self.assertEqual(packet[7:11], b'\x00\x00\x00\x00') # Padding

    def test_rise_mode_clamping(self):
        extreme_metrics = MockHardwareMetrics(cpu_temp=120, gpu_temp=-10, cpu_load=150, ram_load=-5)
        packet = build_rise_mode_packet(255, extreme_metrics)
        self.assertEqual(packet[1], 99) # Max display val 99
        self.assertEqual(packet[2], 0)  # Min gpu temp 0
        self.assertEqual(packet[3], 99) # Max cpu load 99
        self.assertEqual(packet[4], 0)  # Min ram load 0

    def test_deepcool_packet_structure(self):
        packet = build_deepcool_packet(55, self.metrics)
        self.assertEqual(len(packet), 64)
        self.assertEqual(packet[0], 0x00)
        self.assertEqual(packet[1], 0xDA)
        self.assertEqual(packet[2], 55)
        self.assertEqual(packet[3], 22) # CPU load
        self.assertEqual(packet[4], 0x01)
        self.assertEqual(packet[5:], bytes(59))

    def test_nzxt_kraken_packet_structure(self):
        packet = build_nzxt_kraken_packet(62, self.metrics)
        self.assertEqual(len(packet), 64)
        self.assertEqual(packet[0], 0x00)
        self.assertEqual(packet[1], 0x70)
        self.assertEqual(packet[2], 0x01)
        self.assertEqual(packet[3], 62)
        self.assertEqual(packet[4], 42) # GPU temp

    def test_lian_li_packet_structure(self):
        packet = build_lian_li_packet(70, self.metrics)
        self.assertEqual(len(packet), 64)
        self.assertEqual(packet[0], 0x00)
        self.assertEqual(packet[1], 0xE0)
        self.assertEqual(packet[2], 70)
        self.assertEqual(packet[3], 42)

    def test_cooler_master_packet_structure(self):
        packet = build_cooler_master_packet(33, self.metrics)
        self.assertEqual(len(packet), 64)
        self.assertEqual(packet[0], 0x00)
        self.assertEqual(packet[1], 0x55)
        self.assertEqual(packet[2], 33)
        self.assertEqual(packet[3], 22)


class TestDisplayModes(unittest.TestCase):
    def setUp(self):
        self.metrics = MockHardwareMetrics(cpu_temp=62.4, gpu_temp=74.8, ssd_temp=41.2, cpu_load=35.0, ram_load=60.0)

    def test_mode_cpu_temp(self):
        val = clamp(self.metrics.cpu_temp)
        self.assertEqual(val, 62)

    def test_mode_gpu_temp(self):
        val = clamp(self.metrics.gpu_temp)
        self.assertEqual(val, 75)

    def test_mode_highest_temp(self):
        val = clamp(max(self.metrics.cpu_temp, self.metrics.gpu_temp))
        self.assertEqual(val, 75)

    def test_mode_ssd_temp(self):
        val = clamp(self.metrics.ssd_temp)
        self.assertEqual(val, 41)

    def test_mode_ssd_fallback_when_zero(self):
        zero_ssd = MockHardwareMetrics(ssd_temp=0)
        fallback = zero_ssd.ssd_temp if zero_ssd.ssd_temp > 0 else 38.0
        self.assertEqual(fallback, 38.0)

    def test_mode_alternating(self):
        # Simula alternância
        counter = 0
        interval = 1
        alternate_seconds = 5
        show_cpu = True

        for sec in range(12):
            counter += interval
            if counter >= alternate_seconds:
                counter = 0
                show_cpu = not show_cpu

        # 0s..4s: CPU, 5s..9s: GPU, 10s..: CPU
        self.assertTrue(show_cpu)

    def test_manual_slider_override(self):
        # Quando o slider manual estiver ativo, o valor manual sobrepõe os sensores
        is_manual_active = True
        slider_value = 88
        display_val = slider_value if is_manual_active else clamp(self.metrics.cpu_temp)
        self.assertEqual(display_val, 88)

    def test_blink_visual_alert(self):
        # Se temp >= 85 e visual blink ativo, o valor deve piscar para 99 alternadamente
        hot_metrics = MockHardwareMetrics(cpu_temp=87.0)
        visual_blink_enabled = True
        blink_state = False

        if visual_blink_enabled and hot_metrics.cpu_temp >= 85:
            blink_state = not blink_state
            sent_val = 99 if blink_state else clamp(hot_metrics.cpu_temp)

        self.assertEqual(sent_val, 99)


class TestTelemetryCalculation(unittest.TestCase):
    def test_ram_formatting(self):
        # Simula leitura Win32 GlobalMemoryStatusEx: 16492674457 bytes usados de 34359738368 bytes (32 GB)
        total_bytes = 34359738368
        avail_bytes = 17867063911
        used_bytes = total_bytes - avail_bytes

        total_gb = total_bytes / (1024 ** 3)
        used_gb = used_bytes / (1024 ** 3)
        load_pct = (used_bytes / total_bytes) * 100.0

        self.assertAlmostEqual(total_gb, 32.0, places=1)
        self.assertAlmostEqual(used_gb, 15.4, places=1)
        self.assertAlmostEqual(load_pct, 48.0, places=0)

    def test_tray_color_thresholds(self):
        def get_tray_color(temp):
            if temp >= 80:
                return "RED"
            elif temp >= 65:
                return "ORANGE"
            else:
                return "CYAN"

        self.assertEqual(get_tray_color(45), "CYAN")
        self.assertEqual(get_tray_color(64.9), "CYAN")
        self.assertEqual(get_tray_color(65), "ORANGE")
        self.assertEqual(get_tray_color(79.9), "ORANGE")
        self.assertEqual(get_tray_color(80), "RED")
        self.assertEqual(get_tray_color(95), "RED")


class TestStandaloneAndExtendedTelemetry(unittest.TestCase):
    def setUp(self):
        self.metrics = MockHardwareMetrics(
            cpu_name="AMD Ryzen 7 7800X3D",
            gpu_name="NVIDIA GeForce RTX 4070 SUPER",
            cpu_temp=54.2,
            gpu_temp=48.0,
            cpu_load=18.0,
            gpu_load=30.0,
            cpu_clock_ghz=4.85,
            cpu_power_w=72.0,
            gpu_power_w=185.0,
            gpu_vram_used_gb=6.2,
            gpu_vram_total_gb=12.0,
            ram_used_gb=14.8,
            ram_total_gb=32.0,
            ram_load=46.2,
            ssd_temp=39.0,
            mb_temp=33.0,
            cpu_fan_rpm=1100,
            gpu_fan_rpm=950
        )

    def test_standalone_device_header(self):
        active_device = None
        header_text = active_device.DeviceName if active_device else "🖥️ Monitor de Hardware do Sistema (Modo Standalone)"
        self.assertIn("Modo Standalone", header_text)

    def test_standalone_tray_status(self):
        active_device = None
        status = active_device.DeviceName if active_device else "Monitor de PC"
        self.assertEqual(status, "Monitor de PC")

    def test_tray_tooltip_length_safety(self):
        # O tooltip deve ser sempre <= 63 chars para compatibilidade com a Win32 API NotifyIcon
        # Caso 1: Status padrão (<= 63 chars)
        status = "Monitor de PC"
        tooltip = f"Vizzor ({status})\nCPU: {self.metrics.cpu_temp:.0f}°C ({self.metrics.cpu_load:.0f}%)\nGPU: {self.metrics.gpu_temp:.0f}°C ({self.metrics.gpu_load:.0f}%)\nRAM: {self.metrics.ram_load:.0f}%"
        if len(tooltip) >= 64:
            tooltip = f"Vizzor: CPU {self.metrics.cpu_temp:.0f}°C | GPU {self.metrics.gpu_temp:.0f}°C | RAM {self.metrics.ram_load:.0f}%"
            if len(tooltip) >= 64:
                tooltip = tooltip[:63]

        self.assertLess(len(tooltip), 64)
        self.assertTrue(tooltip.startswith("Vizzor ("))

        # Caso 2: Status muito longo (força fallback compacto)
        status_longo = "Rise Mode Aura Ice 360 (RM-WAIB-06-ARGB)"
        tooltip_long = f"Vizzor ({status_longo})\nCPU: {self.metrics.cpu_temp:.0f}°C ({self.metrics.cpu_load:.0f}%)\nGPU: {self.metrics.gpu_temp:.0f}°C ({self.metrics.gpu_load:.0f}%)\nRAM: {self.metrics.ram_load:.0f}%\nVisor: 55"
        if len(tooltip_long) >= 64:
            tooltip_long = f"Vizzor: CPU {self.metrics.cpu_temp:.0f}°C | GPU {self.metrics.gpu_temp:.0f}°C | RAM {self.metrics.ram_load:.0f}%"
            if len(tooltip_long) >= 64:
                tooltip_long = tooltip_long[:63]

        self.assertLess(len(tooltip_long), 64)
        self.assertTrue(tooltip_long.startswith("Vizzor:"))

    def test_cpu_detail_formatting(self):
        cpu_name = self.metrics.cpu_name
        cpu_clock = f"{self.metrics.cpu_clock_ghz:.2f} GHz" if self.metrics.cpu_clock_ghz > 0 else ""
        cpu_power = f"{self.metrics.cpu_power_w:.0f} W" if self.metrics.cpu_power_w > 0 else ""

        detail = cpu_name
        if cpu_clock: detail += f" • {cpu_clock}"
        if cpu_power: detail += f" • {cpu_power}"

        self.assertEqual(detail, "AMD Ryzen 7 7800X3D • 4.85 GHz • 72 W")

    def test_gpu_detail_formatting(self):
        gpu_name = self.metrics.gpu_name
        gpu_vram = f"VRAM: {self.metrics.gpu_vram_used_gb:.1f}/{self.metrics.gpu_vram_total_gb:.1f} GB"
        gpu_power = f"{self.metrics.gpu_power_w:.0f} W"
        gpu_fan = f"Fan: {self.metrics.gpu_fan_rpm:.0f} RPM"

        detail = f"{gpu_name} • {gpu_vram} • {gpu_power} • {gpu_fan}"
        self.assertEqual(detail, "NVIDIA GeForce RTX 4070 SUPER • VRAM: 6.2/12.0 GB • 185 W • Fan: 950 RPM")

    def test_ram_detail_formatting(self):
        ram_text = f"{self.metrics.ram_used_gb:.1f} / {self.metrics.ram_total_gb:.1f} GB ({self.metrics.ram_load:.0f}%)"
        self.assertEqual(ram_text, "14.8 / 32.0 GB (46%)")

    def test_fallback_when_sensors_empty(self):
        empty_metrics = MockHardwareMetrics(
            cpu_name="", gpu_name="", cpu_clock_ghz=0, cpu_power_w=0,
            gpu_vram_used_gb=0, gpu_vram_total_gb=0, gpu_power_w=0, gpu_fan_rpm=0
        )
        cpu_name = empty_metrics.cpu_name if empty_metrics.cpu_name else "CPU"
        self.assertEqual(cpu_name, "CPU")
        gpu_name = empty_metrics.gpu_name if empty_metrics.gpu_name else "GPU"
        self.assertEqual(gpu_name, "GPU")


if __name__ == '__main__':
    unittest.main(verbosity=2)
