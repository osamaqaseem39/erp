#!/usr/bin/env python3
"""Generate stub service implementations for missing ERP interfaces."""
import re
from pathlib import Path

ROOT = Path(r"D:\Projects\erp")

STUBS = [
    ("ERP.Application/Interfaces/Finance/IAccountingService.cs", "ERP.Application.Finance", "ERP.Application/Finance"),
    ("ERP.Application/Interfaces/Finance/IBankService.cs", "ERP.Application.Finance", "ERP.Application/Finance"),
    ("ERP.Application/Interfaces/Finance/IFixedAssetService.cs", "ERP.Application.Finance", "ERP.Application/Finance"),
    ("ERP.Application/Interfaces/Finance/IBillService.cs", "ERP.Application.Finance", "ERP.Application/Finance"),
    ("ERP.Application/Interfaces/HRM/IAttendanceService.cs", "ERP.Application.HRM", "ERP.Application/HRM"),
    ("ERP.Application/Interfaces/HRM/IBenefitsService.cs", "ERP.Application.HRM", "ERP.Application/HRM"),
    ("ERP.Application/Interfaces/HRM/ILeaveRequestService.cs", "ERP.Application.HRM", "ERP.Application/HRM"),
    ("ERP.Application/Interfaces/HRM/IDepartmentService.cs", "ERP.Application.HRM", "ERP.Application/HRM"),
    ("ERP.Application/Interfaces/Inventory/IItemCategoryService.cs", "ERP.Application.Inventory", "ERP.Application/Inventory"),
    ("ERP.Application/Interfaces/Inventory/IStockAdjustmentService.cs", "ERP.Application.Inventory", "ERP.Application/Inventory"),
    ("ERP.Application/Interfaces/Procurement/IPurchaseOrderService.cs", "ERP.Application.Procurement", "ERP.Application/Procurement"),
    ("ERP.Application/Interfaces/Procurement/IPurchaseInvoiceService.cs", "ERP.Application.Procurement", "ERP.Application/Procurement"),
    ("ERP.Application/Interfaces/Procurement/IGoodsReceiptNoteService.cs", "ERP.Application.Procurement", "ERP.Application/Procurement"),
    ("ERP.Application/Interfaces/Procurement/IPurchaseRequisitionService.cs", "ERP.Application.Procurement", "ERP.Application/Procurement"),
]

METHOD_RE = re.compile(r"^\s*(Task(?:<[^;]+?)?>)\s+(\w+)\s*\(([^)]*)\)\s*;", re.M)


def default_body(ret: str, class_name: str, name: str) -> str:
    if ret == "Task":
        return "        return Task.CompletedTask;"
    if ret.startswith("Task<bool>"):
        return "        return Task.FromResult(false);"
    if ret.startswith("Task<Guid>"):
        return "        return Task.FromResult(Guid.Empty);"
    if ret.startswith("Task<decimal>"):
        return "        return Task.FromResult(0m);"
    if ret.startswith("Task<int>"):
        return "        return Task.FromResult(0);"
    if "Task<string" in ret:
        return "        return Task.FromResult(string.Empty);"
    if "Task<IEnumerable<" in ret:
        inner = ret[len("Task<") : -1]
        return f"        return Task.FromResult(Enumerable.Empty<object>() as {inner})!;"
    if ret.startswith("Task<object>"):
        return "        return Task.FromResult<object>(new { });"
    if "?>" in ret or ret.endswith("?>"):
        # Task<Something?>
        inner = ret[5:-1]  # strip Task< >
        return f"        return Task.FromResult<{inner}>(null);"
    if ret.startswith("Task<") and ret.endswith(">"):
        return f'        throw new NotImplementedException("{class_name}.{name} is not yet implemented");'
    return f'        throw new NotImplementedException("{class_name}.{name} is not yet implemented");'


def generate(iface_path: Path, ns: str, out_dir: Path):
    text = iface_path.read_text(encoding="utf-8")
    m = re.search(r"interface (I\w+)", text)
    if not m:
        return
    iface = m.group(1)
    class_name = iface[1:]
    out_file = out_dir / f"{class_name}.cs"
    if out_file.exists():
        print(f"exists {class_name}")
        return

    usings = sorted(set(re.findall(r"using [^;]+;", text)))
    methods = METHOD_RE.findall(text)

    lines = usings + ["using System;", "using System.Linq;", "", f"namespace {ns};", ""]
    lines.append(f"/// <summary>DI stub for {iface}. Replace with real business logic incrementally.</summary>")
    lines.append(f"public class {class_name} : {iface}")
    lines.append("{")
    for ret, name, params in methods:
        lines.append(f"    public {ret} {name}({params})")
        lines.append("    {")
        lines.append(default_body(ret, class_name, name))
        lines.append("    }")
        lines.append("")
    lines.append("}")
    out_dir.mkdir(parents=True, exist_ok=True)
    out_file.write_text("\n".join(lines) + "\n", encoding="utf-8")
    print(f"Created {out_file}")


for rel, ns, out in STUBS:
    generate(ROOT / rel, ns, ROOT / out)

print("Done")
