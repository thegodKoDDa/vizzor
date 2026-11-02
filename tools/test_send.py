"""
Script de teste para envio direto de dados ao visor Rise Mode Aura Ice.
Requer: pip install hidapi (no Windows ou Linux com acesso ao USB)

Uso:
    python test_send.py --cpu 55 --gpu 48 --cpu-load 30
"""

import sys
import time
import argparse

try:
    import hid
except ImportError:
    print("Biblioteca 'hidapi' não encontrada. Instale com: pip install hidapi")
    sys.exit(1)

VENDOR_ID = 0xAA88   # 43656
PRODUCT_ID = 0x8666  # 34406

def build_packet(cpu_temp=0, gpu_temp=0, cpu_load=0, mem_load=0, mb_temp=0, gpu_load=0):
    """
    Constrói o pacote de 24 bytes conforme especificação do protocolo:
    Byte 0: Report ID (0x00)
    Byte 1: CPU Temp (°C)
    Byte 2: GPU Temp (°C)
    Byte 3: CPU Usage (%)
    Byte 4: RAM Usage (%)
    Byte 5: Motherboard Temp (°C)
    Byte 6: GPU Usage (%)
    Bytes 7-23: Zeros (padding)
    """
    packet = bytearray(24)
    packet[0] = 0x00
    packet[1] = int(cpu_temp) & 0xFF
    packet[2] = int(gpu_temp) & 0xFF
    packet[3] = int(cpu_load) & 0xFF
    packet[4] = int(mem_load) & 0xFF
    packet[5] = int(mb_temp) & 0xFF
    packet[6] = int(gpu_load) & 0xFF
    return bytes(packet)

def main():
    parser = argparse.ArgumentParser(description="Envia métricas de teste para o visor Rise Mode Aura Ice.")
    parser.add_argument("--cpu", type=int, default=50, help="Temperatura da CPU em °C (padrão: 50)")
    parser.add_argument("--gpu", type=int, default=45, help="Temperatura da GPU em °C (padrão: 45)")
    parser.add_argument("--cpu-load", type=int, default=25, help="Uso de CPU em % (padrão: 25)")
    parser.add_argument("--ram-load", type=int, default=50, help="Uso de RAM em % (padrão: 50)")
    parser.add_argument("--mb-temp", type=int, default=35, help="Temperatura da placa-mãe em °C (padrão: 35)")
    parser.add_argument("--gpu-load", type=int, default=10, help="Uso de GPU em % (padrão: 10)")
    parser.add_argument("--interval", type=float, default=1.0, help="Intervalo de repetição em segundos (padrão: 1.0)")
    parser.add_argument("--count", type=int, default=5, help="Quantidade de pacotes a enviar (padrão: 5)")

    args = parser.parse_args()

    print(f"Buscando dispositivo USB HID VID=0x{VENDOR_ID:04X}, PID=0x{PRODUCT_ID:04X}...")
    try:
        device = hid.device()
        device.open(VENDOR_ID, PRODUCT_ID)
        print("Dispositivo conectado com sucesso!")
        print(f"Fabricante: {device.get_manufacturer_string()}")
        print(f"Produto:    {device.get_product_string()}")
        print(f"Serial:     {device.get_serial_number_string()}")
    except Exception as e:
        print(f"Erro ao abrir dispositivo: {e}")
        print("Verifique se o software oficial da Rise Mode está fechado e o serviço 'tifSer' parado.")
        return

    try:
        packet = build_packet(
            cpu_temp=args.cpu,
            gpu_temp=args.gpu,
            cpu_load=args.cpu_load,
            mem_load=args.ram_load,
            mb_temp=args.mb_temp,
            gpu_load=args.gpu_load
        )
        print(f"Enviando pacote: {list(packet)}")

        for i in range(args.count):
            # No Windows hidapi, o primeiro byte é o report ID
            bytes_written = device.write(packet)
            print(f"[{i+1}/{args.count}] Enviados {bytes_written} bytes.")
            time.sleep(args.interval)

    finally:
        device.close()
        print("Conexão fechada.")

if __name__ == "__main__":
    main()
