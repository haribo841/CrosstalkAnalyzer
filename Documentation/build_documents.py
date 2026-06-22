from __future__ import annotations

from pathlib import Path
from typing import Iterable, Sequence

from PIL import Image, ImageColor, ImageDraw, ImageFont
from docx import Document
from docx.enum.section import WD_SECTION
from docx.enum.table import WD_CELL_VERTICAL_ALIGNMENT, WD_TABLE_ALIGNMENT
from docx.enum.text import WD_ALIGN_PARAGRAPH, WD_BREAK, WD_LINE_SPACING
from docx.oxml import OxmlElement
from docx.oxml.ns import qn
from docx.shared import Inches, Pt, RGBColor


ROOT = Path(__file__).resolve().parent
OUTPUT = ROOT / "Final"
ASSETS = ROOT / "Assets"
OUTPUT.mkdir(parents=True, exist_ok=True)
ASSETS.mkdir(parents=True, exist_ok=True)

NAVY = "172B4D"
BLUE = "275DDB"
LIGHT_BLUE = "EAF1FF"
ORANGE = "C2410C"
LIGHT_ORANGE = "FFF3E8"
GREEN = "28734A"
LIGHT_GREEN = "EDF8F1"
INK = "172033"
MUTED = "59677C"
GRID = "DDE3EC"
LIGHT_GRAY = "F4F6FA"
WHITE = "FFFFFF"
BLACK = "000000"

AUTHOR = "Adam Kubiś"
LECTURER = "dr inż. Piotr Górniak"
COURSE = "Kompatybilność elektromagnetyczna"
PROJECT = "EMC Lab Assistant"
VERSION = "1.0"
DATE = "czerwiec 2026"


def rgb(hex_color: str) -> RGBColor:
    return RGBColor.from_string(hex_color)


def set_run_font(run, name="Aptos", size=10.5, color=INK, bold=False, italic=False):
    run.font.name = name
    run._element.get_or_add_rPr().rFonts.set(qn("w:ascii"), name)
    run._element.get_or_add_rPr().rFonts.set(qn("w:hAnsi"), name)
    run._element.get_or_add_rPr().rFonts.set(qn("w:eastAsia"), name)
    run.font.size = Pt(size)
    run.font.color.rgb = rgb(color)
    run.bold = bold
    run.italic = italic


def shade_cell(cell, fill: str):
    tc_pr = cell._tc.get_or_add_tcPr()
    shd = tc_pr.find(qn("w:shd"))
    if shd is None:
        shd = OxmlElement("w:shd")
        tc_pr.append(shd)
    shd.set(qn("w:fill"), fill)


def set_cell_margins(cell, top=80, start=120, bottom=80, end=120):
    tc = cell._tc
    tc_pr = tc.get_or_add_tcPr()
    tc_mar = tc_pr.first_child_found_in("w:tcMar")
    if tc_mar is None:
        tc_mar = OxmlElement("w:tcMar")
        tc_pr.append(tc_mar)
    for margin, value in (("top", top), ("start", start), ("bottom", bottom), ("end", end)):
        node = tc_mar.find(qn(f"w:{margin}"))
        if node is None:
            node = OxmlElement(f"w:{margin}")
            tc_mar.append(node)
        node.set(qn("w:w"), str(value))
        node.set(qn("w:type"), "dxa")


def set_cell_width(cell, dxa: int):
    tc_pr = cell._tc.get_or_add_tcPr()
    tc_w = tc_pr.find(qn("w:tcW"))
    if tc_w is None:
        tc_w = OxmlElement("w:tcW")
        tc_pr.append(tc_w)
    tc_w.set(qn("w:w"), str(dxa))
    tc_w.set(qn("w:type"), "dxa")


def set_table_geometry(table, widths_dxa: Sequence[int], indent_dxa=120):
    table.alignment = WD_TABLE_ALIGNMENT.LEFT
    table.autofit = False
    tbl_pr = table._tbl.tblPr
    tbl_w = tbl_pr.find(qn("w:tblW"))
    if tbl_w is None:
        tbl_w = OxmlElement("w:tblW")
        tbl_pr.append(tbl_w)
    tbl_w.set(qn("w:w"), str(sum(widths_dxa)))
    tbl_w.set(qn("w:type"), "dxa")

    tbl_ind = tbl_pr.find(qn("w:tblInd"))
    if tbl_ind is None:
        tbl_ind = OxmlElement("w:tblInd")
        tbl_pr.append(tbl_ind)
    tbl_ind.set(qn("w:w"), str(indent_dxa))
    tbl_ind.set(qn("w:type"), "dxa")

    grid = table._tbl.tblGrid
    for child in list(grid):
        grid.remove(child)
    for width in widths_dxa:
        col = OxmlElement("w:gridCol")
        col.set(qn("w:w"), str(width))
        grid.append(col)

    for row in table.rows:
        for index, cell in enumerate(row.cells):
            set_cell_width(cell, widths_dxa[index])
            set_cell_margins(cell)
            cell.vertical_alignment = WD_CELL_VERTICAL_ALIGNMENT.CENTER


def set_repeat_table_header(row):
    tr_pr = row._tr.get_or_add_trPr()
    tbl_header = OxmlElement("w:tblHeader")
    tbl_header.set(qn("w:val"), "true")
    tr_pr.append(tbl_header)


def set_paragraph_shading(paragraph, fill: str, border: str | None = None):
    p_pr = paragraph._p.get_or_add_pPr()
    shd = p_pr.find(qn("w:shd"))
    if shd is None:
        shd = OxmlElement("w:shd")
        p_pr.append(shd)
    shd.set(qn("w:fill"), fill)
    if border:
        p_bdr = p_pr.find(qn("w:pBdr"))
        if p_bdr is None:
            p_bdr = OxmlElement("w:pBdr")
            p_pr.append(p_bdr)
        for edge in ("top", "left", "bottom", "right"):
            node = OxmlElement(f"w:{edge}")
            node.set(qn("w:val"), "single")
            node.set(qn("w:sz"), "6")
            node.set(qn("w:space"), "4")
            node.set(qn("w:color"), border)
            p_bdr.append(node)


def add_page_number(paragraph):
    paragraph.alignment = WD_ALIGN_PARAGRAPH.RIGHT
    run = paragraph.add_run("Strona ")
    set_run_font(run, size=8.5, color=MUTED)
    fld_char1 = OxmlElement("w:fldChar")
    fld_char1.set(qn("w:fldCharType"), "begin")
    instr_text = OxmlElement("w:instrText")
    instr_text.set(qn("xml:space"), "preserve")
    instr_text.text = " PAGE "
    fld_char2 = OxmlElement("w:fldChar")
    fld_char2.set(qn("w:fldCharType"), "end")
    run._r.append(fld_char1)
    run._r.append(instr_text)
    run._r.append(fld_char2)


def setup_document(doc: Document, short_title: str, preset="standard"):
    section = doc.sections[0]
    section.page_width = Inches(8.5)
    section.page_height = Inches(11)
    section.top_margin = Inches(0.82)
    section.bottom_margin = Inches(0.78)
    section.left_margin = Inches(0.9)
    section.right_margin = Inches(0.9)
    section.header_distance = Inches(0.36)
    section.footer_distance = Inches(0.36)

    styles = doc.styles
    normal = styles["Normal"]
    normal.font.name = "Aptos"
    normal._element.rPr.rFonts.set(qn("w:ascii"), "Aptos")
    normal._element.rPr.rFonts.set(qn("w:hAnsi"), "Aptos")
    normal.font.size = Pt(10.5 if preset == "standard" else 10.25)
    normal.font.color.rgb = rgb(INK)
    normal.paragraph_format.space_after = Pt(5 if preset == "standard" else 4)
    normal.paragraph_format.line_spacing = 1.12 if preset == "standard" else 1.18

    for name, size, color, before, after in [
        ("Title", 28, NAVY, 0, 10),
        ("Subtitle", 13, MUTED, 0, 12),
        ("Heading 1", 16, NAVY, 16, 7),
        ("Heading 2", 13, BLUE, 12, 5),
        ("Heading 3", 11.5, NAVY, 9, 4),
    ]:
        style = styles[name]
        style.font.name = "Aptos Display" if name in {"Title", "Heading 1"} else "Aptos"
        style._element.rPr.rFonts.set(qn("w:ascii"), style.font.name)
        style._element.rPr.rFonts.set(qn("w:hAnsi"), style.font.name)
        style.font.size = Pt(size)
        style.font.color.rgb = rgb(color)
        style.font.bold = name != "Subtitle"
        style.paragraph_format.space_before = Pt(before)
        style.paragraph_format.space_after = Pt(after)
        style.paragraph_format.keep_with_next = True

    header = section.header
    hp = header.paragraphs[0]
    hp.alignment = WD_ALIGN_PARAGRAPH.LEFT
    run = hp.add_run(f"{PROJECT}  |  {short_title}")
    set_run_font(run, size=8.5, color=MUTED, bold=True)

    footer = section.footer
    footer.paragraphs[0].clear()
    add_page_number(footer.paragraphs[0])

    doc.core_properties.title = short_title
    doc.core_properties.subject = PROJECT
    doc.core_properties.author = AUTHOR
    doc.core_properties.keywords = "C#, Avalonia UI, EMC, kompatybilność elektromagnetyczna"


def add_cover(doc: Document, title: str, subtitle: str, doc_type: str, metadata: Sequence[tuple[str, str]]):
    p = doc.add_paragraph()
    p.paragraph_format.space_before = Pt(90)
    p.alignment = WD_ALIGN_PARAGRAPH.CENTER
    r = p.add_run("KOMPATYBILNOŚĆ ELEKTROMAGNETYCZNA")
    set_run_font(r, size=10, color=BLUE, bold=True)

    p = doc.add_paragraph()
    p.style = doc.styles["Title"]
    p.alignment = WD_ALIGN_PARAGRAPH.CENTER
    p.add_run(title)

    p = doc.add_paragraph()
    p.style = doc.styles["Subtitle"]
    p.alignment = WD_ALIGN_PARAGRAPH.CENTER
    p.add_run(subtitle)

    p = doc.add_paragraph()
    p.alignment = WD_ALIGN_PARAGRAPH.CENTER
    p.paragraph_format.space_before = Pt(10)
    r = p.add_run(doc_type.upper())
    set_run_font(r, size=10, color=ORANGE, bold=True)

    doc.add_paragraph().paragraph_format.space_after = Pt(32)

    for label, value in metadata:
        p = doc.add_paragraph()
        p.paragraph_format.left_indent = Inches(1.05)
        p.paragraph_format.right_indent = Inches(0.75)
        p.paragraph_format.space_after = Pt(3)
        label_run = p.add_run(f"{label}: ")
        set_run_font(label_run, size=9.8, color=NAVY, bold=True)
        value_run = p.add_run(value)
        set_run_font(value_run, size=9.8, color=INK)

    p = doc.add_paragraph()
    p.paragraph_format.space_before = Pt(60)
    p.alignment = WD_ALIGN_PARAGRAPH.CENTER
    r = p.add_run("Politechnika Poznańska  •  " + DATE)
    set_run_font(r, size=9.5, color=MUTED)
    doc.add_page_break()


def add_heading(doc: Document, text: str, level=1):
    return doc.add_heading(text, level=level)


def add_body(doc: Document, text: str, bold_prefix: str | None = None, align=None):
    p = doc.add_paragraph()
    if align is not None:
        p.alignment = align
    if bold_prefix and text.startswith(bold_prefix):
        r1 = p.add_run(bold_prefix)
        set_run_font(r1, bold=True)
        r2 = p.add_run(text[len(bold_prefix):])
        set_run_font(r2)
    else:
        r = p.add_run(text)
        set_run_font(r)
    return p


def add_bullets(doc: Document, items: Iterable[str], compact=False):
    for item in items:
        p = doc.add_paragraph(style="List Bullet")
        p.paragraph_format.left_indent = Inches(0.36)
        p.paragraph_format.first_line_indent = Inches(-0.18)
        p.paragraph_format.space_after = Pt(2 if compact else 4)
        p.paragraph_format.line_spacing = 1.12
        r = p.add_run(item)
        set_run_font(r)


def add_steps(doc: Document, items: Iterable[str]):
    for item in items:
        p = doc.add_paragraph(style="List Number")
        p.paragraph_format.left_indent = Inches(0.42)
        p.paragraph_format.first_line_indent = Inches(-0.22)
        p.paragraph_format.space_after = Pt(5)
        r = p.add_run(item)
        set_run_font(r)


def add_callout(doc: Document, label: str, text: str, kind="info"):
    colors = {
        "info": (LIGHT_BLUE, BLUE),
        "warning": (LIGHT_ORANGE, ORANGE),
        "success": (LIGHT_GREEN, GREEN),
    }
    fill, border = colors[kind]
    p = doc.add_paragraph()
    p.paragraph_format.left_indent = Inches(0.08)
    p.paragraph_format.right_indent = Inches(0.08)
    p.paragraph_format.space_before = Pt(4)
    p.paragraph_format.space_after = Pt(8)
    set_paragraph_shading(p, fill, border)
    r = p.add_run(label + ": ")
    set_run_font(r, color=border, bold=True)
    r = p.add_run(text)
    set_run_font(r, color=INK)


def add_equation(doc: Document, equation: str, explanation: str | None = None):
    p = doc.add_paragraph()
    p.alignment = WD_ALIGN_PARAGRAPH.CENTER
    p.paragraph_format.space_before = Pt(5)
    p.paragraph_format.space_after = Pt(5)
    set_paragraph_shading(p, LIGHT_GRAY, GRID)
    r = p.add_run(equation)
    set_run_font(r, name="Cambria Math", size=11.5, color=NAVY, bold=True)
    if explanation:
        p2 = doc.add_paragraph()
        p2.alignment = WD_ALIGN_PARAGRAPH.CENTER
        p2.paragraph_format.space_after = Pt(7)
        r2 = p2.add_run(explanation)
        set_run_font(r2, size=9, color=MUTED, italic=True)


def add_table(doc: Document, headers: Sequence[str], rows: Sequence[Sequence[str]], widths_dxa: Sequence[int]):
    table = doc.add_table(rows=1, cols=len(headers))
    table.style = "Table Grid"
    set_table_geometry(table, widths_dxa)
    set_repeat_table_header(table.rows[0])
    for index, header in enumerate(headers):
        cell = table.rows[0].cells[index]
        shade_cell(cell, NAVY)
        cell.text = header
        p = cell.paragraphs[0]
        p.alignment = WD_ALIGN_PARAGRAPH.CENTER
        for run in p.runs:
            set_run_font(run, size=9, color=WHITE, bold=True)

    for row_index, values in enumerate(rows):
        cells = table.add_row().cells
        for index, value in enumerate(values):
            cells[index].text = str(value)
            if row_index % 2:
                shade_cell(cells[index], "F8FAFD")
            p = cells[index].paragraphs[0]
            p.alignment = WD_ALIGN_PARAGRAPH.LEFT if index == 0 else WD_ALIGN_PARAGRAPH.CENTER
            for run in p.runs:
                set_run_font(run, size=8.8)
    set_table_geometry(table, widths_dxa)
    doc.add_paragraph().paragraph_format.space_after = Pt(1)
    return table


def add_image(doc: Document, image_path: Path, caption: str, width=6.15):
    p = doc.add_paragraph()
    p.alignment = WD_ALIGN_PARAGRAPH.CENTER
    p.paragraph_format.keep_with_next = True
    run = p.add_run()
    run.add_picture(str(image_path), width=Inches(width))
    doc_pr = doc.inline_shapes[-1]._inline.docPr
    doc_pr.set("descr", caption)
    doc_pr.set("title", image_path.stem.replace("_", " "))
    c = doc.add_paragraph()
    c.alignment = WD_ALIGN_PARAGRAPH.CENTER
    c.paragraph_format.space_after = Pt(8)
    r = c.add_run(caption)
    set_run_font(r, size=8.8, color=MUTED, italic=True)


def add_code(doc: Document, lines: Sequence[str]):
    p = doc.add_paragraph()
    p.paragraph_format.left_indent = Inches(0.14)
    p.paragraph_format.right_indent = Inches(0.14)
    p.paragraph_format.space_after = Pt(8)
    set_paragraph_shading(p, "F2F4F7", GRID)
    for index, line in enumerate(lines):
        if index:
            p.add_run().add_break()
        r = p.add_run(line)
        set_run_font(r, name="Consolas", size=8.7, color=INK)


def load_font(size: int, bold=False):
    choices = [
        Path("C:/Windows/Fonts/arialbd.ttf" if bold else "C:/Windows/Fonts/arial.ttf"),
        Path("C:/Windows/Fonts/seguisb.ttf" if bold else "C:/Windows/Fonts/segoeui.ttf"),
    ]
    for path in choices:
        if path.exists():
            return ImageFont.truetype(str(path), size)
    return ImageFont.load_default()


def draw_centered_text(draw, rect, text, font, fill, max_width=None):
    x1, y1, x2, y2 = rect
    words = text.split()
    lines, line = [], ""
    target = max_width or (x2 - x1 - 30)
    for word in words:
        candidate = (line + " " + word).strip()
        if draw.textbbox((0, 0), candidate, font=font)[2] <= target:
            line = candidate
        else:
            if line:
                lines.append(line)
            line = word
    if line:
        lines.append(line)
    heights = [draw.textbbox((0, 0), item, font=font)[3] for item in lines]
    total = sum(heights) + max(0, len(lines) - 1) * 5
    y = y1 + (y2 - y1 - total) / 2
    for item, height in zip(lines, heights):
        bbox = draw.textbbox((0, 0), item, font=font)
        x = x1 + (x2 - x1 - (bbox[2] - bbox[0])) / 2
        draw.text((x, y), item, font=font, fill=fill)
        y += height + 5


def create_diagrams():
    for color in [
        NAVY, BLUE, LIGHT_BLUE, ORANGE, LIGHT_ORANGE, GREEN, LIGHT_GREEN,
        INK, MUTED, GRID, LIGHT_GRAY, WHITE, BLACK, "F8FAFD", "FAFBFD",
        "8FB3FF", "D9E4F7", "AFC0DA", "30486F", "4F8CFF", "E9EEF8",
    ]:
        ImageColor.colormap[color.lower()] = f"#{color}"

    font = load_font(28)
    small = load_font(22)
    bold = load_font(28, True)
    title = load_font(34, True)

    # Architecture
    img = Image.new("RGB", (1600, 850), WHITE)
    d = ImageDraw.Draw(img)
    d.text((60, 40), "Architektura logiczna EMC Lab Assistant", font=title, fill=NAVY)
    blocks = [
        ((80, 150, 1520, 280), "Warstwa prezentacji (Avalonia XAML)\nMainWindow, wybór scenariusza, widoki kroków", LIGHT_BLUE, BLUE),
        ((80, 340, 1520, 490), "Warstwa ViewModel (MVVM)\nnawigacja, walidacja, stan kreatorów, komendy", "F4F6FA", NAVY),
        ((80, 550, 740, 720), "Modele danych\npunkty pomiarowe, wyniki, statystyki", LIGHT_GREEN, GREEN),
        ((860, 550, 1520, 720), "Usługi domenowe\nobliczenia, generowanie częstotliwości, eksport CSV", LIGHT_ORANGE, ORANGE),
    ]
    for rect, text, fill, outline in blocks:
        d.rounded_rectangle(rect, radius=22, fill=fill, outline=outline, width=4)
        draw_centered_text(d, rect, text, bold if rect[1] < 500 else font, NAVY)
    for y1, y2 in [(280, 340), (490, 550)]:
        d.line((800, y1 + 10, 800, y2 - 12), fill=MUTED, width=5)
        d.polygon([(790, y2 - 22), (810, y2 - 22), (800, y2 - 8)], fill=MUTED)
    img.save(ASSETS / "architektura.png")

    # Scenario flow
    img = Image.new("RGB", (1600, 950), WHITE)
    d = ImageDraw.Draw(img)
    d.text((60, 35), "Przebieg pracy w aplikacji", font=title, fill=NAVY)
    start = (560, 110, 1040, 205)
    d.rounded_rectangle(start, radius=28, fill=NAVY)
    draw_centered_text(d, start, "Wybór scenariusza", bold, WHITE)
    left_x, right_x = 110, 850
    for x, heading, fill, outline in [
        (left_x, "Przeniki mikropaskowe", LIGHT_BLUE, BLUE),
        (right_x, "Sondy pola bliskiego", LIGHT_ORANGE, ORANGE),
    ]:
        rect = (x, 285, x + 640, 375)
        d.rounded_rectangle(rect, radius=20, fill=fill, outline=outline, width=4)
        draw_centered_text(d, rect, heading, bold, NAVY)
    left_steps = ["1. Dane NEXT/FEXT", "2. Skala liniowa", "3. Błąd analizatora", "4. Statystyka i wykres"]
    right_steps = ["1. Stanowisko", "2. Pomiary 30/50/100 Ω", "3. Pole H i U95", "4. Porównanie trendów"]
    for col_x, steps, fill, outline in [
        (left_x, left_steps, LIGHT_BLUE, BLUE),
        (right_x, right_steps, LIGHT_ORANGE, ORANGE),
    ]:
        for i, step in enumerate(steps):
            y = 420 + i * 105
            rect = (col_x + 45, y, col_x + 595, y + 72)
            d.rounded_rectangle(rect, radius=16, fill=fill, outline=outline, width=2)
            draw_centered_text(d, rect, step, small, INK)
    d.line((800, 205, 430, 285), fill=MUTED, width=4)
    d.line((800, 205, 1170, 285), fill=MUTED, width=4)
    export_rect = (560, 855, 1040, 930)
    d.rounded_rectangle(export_rect, radius=20, fill=LIGHT_GREEN, outline=GREEN, width=4)
    draw_centered_text(d, export_rect, "Eksport wyników do CSV", bold, GREEN)
    d.line((430, 807, 650, 855), fill=MUTED, width=4)
    d.line((1170, 807, 950, 855), fill=MUTED, width=4)
    img.save(ASSETS / "przeplyw_scenariuszy.png")

    # UI map
    img = Image.new("RGB", (1600, 930), "F4F6FA")
    d = ImageDraw.Draw(img)
    d.rounded_rectangle((45, 45, 1555, 885), radius=24, fill=WHITE, outline=GRID, width=4)
    d.rectangle((45, 45, 1555, 220), fill=NAVY)
    d.text((95, 77), "EMC LAB ASSISTANT", font=small, fill="8FB3FF")
    d.text((95, 120), "Tytuł bieżącego kroku", font=title, fill=WHITE)
    d.text((95, 170), "Nazwa scenariusza i krótka instrukcja", font=small, fill="D9E4F7")
    d.rounded_rectangle((1160, 100, 1480, 145), radius=12, fill="30486F")
    d.rounded_rectangle((1160, 100, 1320, 145), radius=12, fill="4F8CFF")
    d.text((1210, 158), "Pasek postępu", font=small, fill=WHITE)
    d.rounded_rectangle((95, 260, 1505, 735), radius=18, fill="FAFBFD", outline=GRID, width=3)
    d.text((125, 290), "Obszar roboczy", font=bold, fill=NAVY)
    for i, label in enumerate(["tabela danych", "wzór / informacja", "wykres / podsumowanie"]):
        x = 145 + i * 440
        d.rounded_rectangle((x, 365, x + 370, 640), radius=16, fill=LIGHT_BLUE if i == 0 else LIGHT_GRAY, outline=GRID, width=2)
        draw_centered_text(d, (x, 365, x + 370, 640), label, font, INK)
    d.rectangle((45, 770, 1555, 885), fill=WHITE)
    d.rounded_rectangle((90, 800, 255, 855), radius=12, fill="E9EEF8")
    d.text((125, 812), "Wstecz", font=small, fill=NAVY)
    d.rounded_rectangle((1305, 800, 1490, 855), radius=12, fill=BLUE)
    d.text((1360, 812), "Dalej", font=small, fill=WHITE)
    img.save(ASSETS / "mapa_interfejsu.png")


def technical_document():
    doc = Document()
    setup_document(doc, "Dokumentacja techniczna", "standard")
    add_cover(
        doc,
        "Dokumentacja techniczna",
        "Architektura, algorytmy, budowanie i utrzymanie programu",
        "Dokumentacja oprogramowania",
        [
            ("Projekt", PROJECT),
            ("Autor", AUTHOR),
            ("Wersja", VERSION),
            ("Technologia", "C# 12, .NET 8, Avalonia UI 12, MVVM"),
            ("Platformy", "Windows 10/11 oraz Linux x64"),
            ("Stan", "Wersja działająca, zweryfikowana kompilacją i testami"),
        ],
    )

    add_heading(doc, "1. Cel i zakres dokumentu")
    add_body(doc, "Dokument opisuje budowę programu EMC Lab Assistant, jego odpowiedzialności funkcjonalne, architekturę, modele danych, algorytmy obliczeniowe, sposób budowania, testowania i publikowania. Jest przeznaczony dla osoby rozwijającej lub oceniającej kod źródłowy.")
    add_callout(doc, "Zakres wersji 1.0", "Program obsługuje dwa kompletne scenariusze laboratoryjne: pomiar przeników między liniami mikropaskowymi oraz sondy pola bliskiego w analizie emisji promieniowanej.", "info")

    add_heading(doc, "2. Wymagania systemowe")
    add_table(
        doc,
        ["Obszar", "Wymaganie", "Realizacja"],
        [
            ("Język", "C# i środowisko .NET", "Projekt SDK-style, TargetFramework net8.0"),
            ("Interfejs", "Graficzny kreator krok po kroku", "Avalonia XAML, cztery kroki w każdym scenariuszu"),
            ("Wieloplatformowość", "Windows i Linux", "Avalonia Desktop oraz publikacja self-contained"),
            ("Walidacja", "Blokada przejścia przy niepełnych danych", "Warunki CanGoNext i komendy MVVM"),
            ("Obliczenia", "Wzory laboratoryjne i niepewność", "Oddzielne usługi CrosstalkLogic i NearFieldLogic"),
            ("Prezentacja", "Tabele, wzory i wykresy", "DataGrid oraz własne kontrolki wykresów"),
            ("Eksport", "Dane do sprawozdania", "Plik CSV UTF-8 z separatorem średnikowym"),
        ],
        [1600, 3300, 4460],
    )

    add_heading(doc, "3. Stos technologiczny")
    add_table(
        doc,
        ["Składnik", "Wersja", "Zastosowanie"],
        [
            ("Microsoft .NET", "8.0", "Runtime, biblioteka standardowa i narzędzia publikowania"),
            ("Avalonia", "12.0.4", "Wieloplatformowy interfejs użytkownika"),
            ("Avalonia.Controls.DataGrid", "12.0.0", "Edycja i prezentacja tabel pomiarowych"),
            ("CommunityToolkit.Mvvm", "8.4.1", "ObservableObject, komendy i generatory właściwości"),
            ("Inter", "pakiet Avalonia", "Spójna typografia interfejsu"),
        ],
        [2400, 1200, 5760],
    )
    add_body(doc, "Avalonia została wybrana zamiast WPF i Windows Forms, ponieważ nie wiąże warstwy prezentacji wyłącznie z systemem Windows. Projekt celuje w .NET 8, dlatego oficjalnym zakresem zgodności są Windows 10/11 oraz wspierane dystrybucje Linuksa.")

    add_heading(doc, "4. Architektura")
    add_image(doc, ASSETS / "architektura.png", "Rysunek 1. Warstwy logiczne programu.")
    add_body(doc, "Aplikacja stosuje wzorzec MVVM. Widoki XAML odpowiadają za prezentację, ViewModele przechowują stan i nawigację, modele opisują rekordy pomiarowe, a usługi wykonują obliczenia oraz eksport. ViewLocator dobiera widok na podstawie typu ViewModelu.")
    add_table(
        doc,
        ["Warstwa / moduł", "Odpowiedzialność"],
        [
            ("MainWindowViewModel", "Wybór scenariusza, bieżący krok, komendy Wstecz/Dalej, restart i zmiana scenariusza."),
            ("ScenarioSelectionViewModel", "Udostępnienie komend uruchamiających jeden z dwóch scenariuszy."),
            ("Step1–Step4ViewModel", "Stan i obliczenia kreatora pomiaru przeników."),
            ("NearFieldStep1–Step4ViewModel", "Stanowisko, dane pomiarowe, pole H, niepewność i podsumowanie sond."),
            ("CrosstalkLogic", "Konwersja dB, błąd analizatora, statystyka i przedział ufności."),
            ("NearFieldLogic", "Przeliczenie mocy na H, niepewność złożona, maksimum i regresja trendu."),
            ("ReportGenerator", "Eksport wyników obu scenariuszy do CSV."),
            ("CrosstalkChart / NearFieldChart", "Rysowanie przebiegów bez zewnętrznej biblioteki wykresowej."),
        ],
        [2750, 6610],
    )

    add_heading(doc, "5. Nawigacja i cykl danych")
    add_image(doc, ASSETS / "przeplyw_scenariuszy.png", "Rysunek 2. Nawigacja od wyboru scenariusza do eksportu.")
    add_steps(doc, [
        "Użytkownik wybiera scenariusz na ekranie startowym.",
        "MainWindowViewModel ustawia właściwy ViewModel pierwszego kroku.",
        "Dane wejściowe są walidowane na bieżąco; komenda Dalej jest aktywna dopiero po spełnieniu wymagań.",
        "Przy przejściu do następnego kroku tworzona jest kolekcja wyników obliczeniowych.",
        "Krok końcowy agreguje statystyki, maksima i trendy oraz udostępnia eksport CSV.",
    ])

    add_heading(doc, "6. Modele danych")
    add_table(
        doc,
        ["Typ", "Najważniejsze dane"],
        [
            ("BandDefinition", "Nazwa pasma, częstotliwość początkowa i końcowa, niepewność analizatora."),
            ("MeasurementPointViewModel", "Częstotliwość oraz surowe wartości NEXT/FEXT."),
            ("MeasurementResult", "Wartości dB, liniowe, Delta Z i granice wyniku."),
            ("StatisticsResult", "Średnia, s, błąd standardowy i 95% przedział ufności."),
            ("NearFieldMeasurementPoint", "f, moce dla 30/50/100 Ω, K oraz Sp."),
            ("NearFieldResult", "Pole H w dBA/m i A/m oraz U95 dla trzech impedancji."),
            ("NearFieldSummary", "Maksimum, częstotliwość maksimum i nachylenie charakterystyki."),
        ],
        [2700, 6660],
    )

    add_heading(doc, "7. Algorytmy scenariusza przeników")
    add_equation(doc, "|Z|lin = 10^(|Z|dB / 20)", "Konwersja modułu transmitancji do skali liniowej.")
    add_equation(doc, "Delta Z = |Z|lin · (10^(UD / 20) - 1)", "Błąd bezwzględny wynikający z niepewności amplitudy analizatora.")
    add_body(doc, "Dla każdego punktu wyznaczane są granice max(0, |Z|lin - Delta Z) oraz |Z|lin + Delta Z. Dla serii NEXT i FEXT program oblicza średnią, odchylenie standardowe z próby, błąd standardowy oraz przedział ufności średniej z rozkładu t-Studenta.")
    add_equation(doc, "CI95 = x̄ ± t(0,975; n-1) · s / √n", "Dla 11 punktów wartość krytyczna wynosi 2,228.")
    add_callout(doc, "Założenie", "W wersji 1.0 niepewność UD jest przypisana do pasma: 0,2 dB dla 1–3 GHz i 0,3 dB dla 7–8 GHz. W zastosowaniu metrologicznym należy ją skonfrontować z poziomem sygnału i tabelą dokładności konkretnego analizatora.", "warning")

    add_heading(doc, "8. Algorytmy scenariusza sond pola bliskiego")
    add_equation(doc, "H[dBA/m] = P[dBm] - 30 + 10·log10(50) - K + Sp", "P - odczyt miernika mocy, K - wzmocnienie toru, Sp - poprawka sondy.")
    add_equation(doc, "H[A/m] = 10^(H[dBA/m] / 20)")
    add_equation(doc, "uH = √(uP² + uK² + uSp² + ur²)", "Składnik ur jest przygotowany w warstwie logiki; w bieżącym kreatorze przyjmuje 0.")
    add_equation(doc, "U95 = k · uH", "Domyślnie uP=0,066 dB, uK=0,2 dB, uSp=0,3 dB, k=2; U95≈0,733 dB.")
    add_body(doc, "Program wyznacza maksimum pola osobno dla linii 30 Ω, 50 Ω i 100 Ω. Szybkość zmian jest szacowana regresją liniową jako dB/100 MHz oraz dB/dekadę częstotliwości.")

    add_heading(doc, "9. Walidacja i obsługa błędów")
    add_bullets(doc, [
        "NEXT i FEXT muszą mieścić się w zakresie od -200 dB do 0 dB.",
        "Moce w scenariuszu sond muszą mieścić się w zakresie od -150 dBm do 30 dBm.",
        "Wszystkie punkty wymagają kompletu wartości K i Sp.",
        "Krok przygotowania stanowiska wymaga zaznaczenia czterech pozycji listy kontrolnej.",
        "Anulowanie okna zapisu nie zmienia stanu analizy.",
        "Błąd zapisu CSV jest prezentowany użytkownikowi w dolnym pasku okna.",
    ])

    add_heading(doc, "10. Eksport danych")
    add_body(doc, "ReportGenerator zapisuje pliki UTF-8 z BOM i separatorem średnikowym, dzięki czemu wyniki są czytelne w polskiej konfiguracji arkusza kalkulacyjnego. Eksport przeników zawiera dane punktowe i statystyki. Eksport sond zawiera warunki środowiskowe, budżet niepewności, wyniki w dBA/m i A/m, granice U95, maksima i trendy.")

    add_heading(doc, "11. Budowanie i publikowanie")
    add_code(doc, [
        "dotnet restore",
        "dotnet build CrosstalkAnalyzer.sln -c Release",
        "dotnet run --project CrosstalkAnalyzer.csproj",
        "dotnet publish -c Release -r win-x64 --self-contained true",
        "dotnet publish -c Release -r linux-x64 --self-contained true",
    ])
    add_callout(doc, "Zgodność", "Samowystarczalna publikacja nie wymaga instalacji .NET na komputerze docelowym, ale nadal podlega wymaganiom systemowym .NET 8 i Avalonia.", "info")

    add_heading(doc, "12. Testy")
    add_table(
        doc,
        ["Obszar", "Sprawdzenie", "Wynik"],
        [
            ("Konwersja dB", "-20 dB = 0,1; -40 dB = 0,01", "zaliczony"),
            ("Delta Z", "Wartość referencyjna dla 0,1 i UD=0,2 dB", "zaliczony"),
            ("Statystyka", "Średnia i s dla serii 1,2,3,4,5", "zaliczony"),
            ("Pole H", "Przykład 200 MHz, P=-34 dBm, K=21,25 dB, Sp=-31 dB", "zaliczony"),
            ("Niepewność", "uH≈0,366546 dB i U95≈0,733092 dB", "zaliczony"),
            ("Eksport", "Obecność wymaganych sekcji w obu plikach CSV", "zaliczony"),
            ("Nawigacja", "Pełne przejście scenariusza sond do kroku 4", "zaliczony"),
            ("Kompilacja", "Release, 0 błędów i 0 ostrzeżeń", "zaliczony"),
        ],
        [1800, 5700, 1860],
    )
    add_body(doc, "Testy mają postać lekkiego programu kontrolnego bez zewnętrznego frameworka testowego. Uruchomienie:")
    add_code(doc, ["dotnet run --project Tests/CrosstalkAnalyzer.CalculationChecks"])

    add_heading(doc, "13. Ograniczenia i kierunki rozwoju")
    add_bullets(doc, [
        "Brak trwałego zapisu sesji; po zamknięciu programu dane pozostają wyłącznie w wyeksportowanym CSV.",
        "Brak importu danych z analizatora i miernika mocy.",
        "Brak automatycznego odczytu K i Sp z pliku charakterystyki; wartości są zapisane jako edytowalne domyślne punkty.",
        "Brak automatycznych testów interfejsu oraz instalatorów dla systemów docelowych.",
        "Możliwe rozszerzenie o kolejne ćwiczenia, raport DOCX/PDF, profile aparatury i zapis projektu.",
    ])

    add_heading(doc, "14. Struktura katalogów")
    add_code(doc, [
        "Models/      - rekordy wejściowe, wyniki i podsumowania",
        "Services/    - logika obliczeń, częstotliwości i eksport",
        "ViewModels/  - stan kreatorów, walidacja i nawigacja",
        "Views/       - widoki Avalonia XAML",
        "Controls/    - własne kontrolki wykresów",
        "Tests/       - kontrole obliczeń, eksportu i nawigacji",
        "Assets/      - zasoby aplikacji",
    ])

    add_heading(doc, "15. Materiały źródłowe")
    add_bullets(doc, [
        "Kod źródłowy projektu EMC Lab Assistant.",
        "R&S ZVL Vector Network Analyzer Data Sheet, wersja 12.00.",
        "R&S HZ-15/HZ-17 Probe Sets i R&S HZ-16 Preamplifier, Product Brochure.",
        "Instrukcja laboratoryjna „Pomiar sondami pola bliskiego”.",
        "„Obliczenia do pomiarów sondami pola bliskiego”.",
        "Wzorzec sprawozdania Lab_KEM_Pom_Emisji_Kabla_PCB.",
    ])

    path = OUTPUT / "01_Dokumentacja_techniczna_EMC_Lab_Assistant.docx"
    doc.save(path)
    return path


def project_report():
    doc = Document()
    setup_document(doc, "Raport projektowy", "standard")
    add_cover(
        doc,
        "EMC Lab Assistant",
        "Wieloplatformowy kreator ćwiczeń laboratoryjnych z kompatybilności elektromagnetycznej",
        "Raport z projektu",
        [
            ("Przedmiot", COURSE),
            ("Autor", AUTHOR),
            ("Prowadzący", LECTURER),
            ("Wersja programu", VERSION),
            ("Technologia", "C# / .NET 8 / Avalonia UI"),
            ("Termin opracowania", DATE),
        ],
    )

    add_heading(doc, "Streszczenie")
    add_body(doc, "Celem projektu było wykonanie aplikacji w języku C#, która prowadzi studenta przez ćwiczenie laboratoryjne, prezentuje stosowane wzory, kontroluje kompletność danych i automatyzuje obliczenia. Zastosowano Avalonia UI, dzięki czemu rozwiązanie działa na systemach Windows i Linux. Wersja 1.0 obejmuje dwa scenariusze: pomiar przeników między liniami mikropaskowymi oraz sondy pola bliskiego w analizie emisji promieniowanej.")
    add_callout(doc, "Rezultat", "Powstał działający program z interfejsem graficznym, dwoma czterostopniowymi kreatorami, wykresami, obliczeniami niepewności i eksportem CSV.", "success")

    add_heading(doc, "1. Uzasadnienie projektu")
    add_body(doc, "Podczas laboratorium student wykonuje wiele powtarzalnych przeliczeń, korzysta z charakterystyk aparatury i musi zachować właściwą kolejność czynności. Błąd przy przepisywaniu danych, pominięcie poprawki lub zastosowanie niewłaściwej jednostki może prowadzić do niepoprawnego sprawozdania. Program pełni rolę asystenta: oddziela pomiar od obliczeń, pokazuje wzór na właściwym etapie oraz umożliwia wyeksportowanie tabeli wynikowej.")

    add_heading(doc, "2. Założenia i wymagania")
    add_bullets(doc, [
        "implementacja w języku C#;",
        "graficzny interfejs prowadzący użytkownika krok po kroku;",
        "działanie na Windows i Linux;",
        "prezentacja wzorów i automatyczne przeliczenia;",
        "walidacja danych wejściowych;",
        "wykresy i podsumowanie wyników;",
        "eksport tabel do dalszej obróbki w sprawozdaniu;",
        "możliwość dołączania kolejnych scenariuszy laboratoryjnych.",
    ])

    add_heading(doc, "3. Wybór technologii")
    add_body(doc, "WPF i Windows Forms są silnie związane z systemem Windows. Avalonia UI wykorzystuje podobny model XAML i pozwala zachować język C# oraz wzorzec MVVM, a jednocześnie udostępnia backendy dla Windows i Linuksa. Projekt został ustawiony na .NET 8, czyli wspieraną wersję LTS, zamiast pierwotnego .NET 9.")
    add_table(
        doc,
        ["Kryterium", "Avalonia UI", "Wpływ na projekt"],
        [
            ("Wieloplatformowość", "Windows, Linux, macOS", "jeden kod interfejsu"),
            ("Język i XAML", "C# oraz deklaratywny XAML", "czytelny podział widok/logika"),
            ("MVVM", "pełne wsparcie wiązań", "łatwa walidacja i testowanie"),
            ("Dystrybucja", "publikacja self-contained", "brak wymogu instalacji runtime"),
        ],
        [2200, 2900, 4260],
    )

    add_heading(doc, "4. Organizacja rozwiązania")
    add_image(doc, ASSETS / "architektura.png", "Rysunek 1. Architektura projektu.")
    add_body(doc, "Rozdzielenie warstw ogranicza sprzężenie interfejsu z algorytmami. Wzory są testowane niezależnie od okna programu, a nowy scenariusz można dodać przez utworzenie modeli, usługi obliczeniowej, zestawu ViewModeli i odpowiadających im widoków.")

    add_heading(doc, "5. Zrealizowane scenariusze")
    add_image(doc, ASSETS / "przeplyw_scenariuszy.png", "Rysunek 2. Dwa scenariusze dostępne w wersji 1.0.")

    add_heading(doc, "5.1. Pomiar przeników między liniami mikropaskowymi", level=2)
    add_steps(doc, [
        "Wybór pasma 1–2 GHz, 2–3 GHz lub 7–8 GHz oraz wprowadzenie NEXT i FEXT.",
        "Konwersja z dB do skali liniowej.",
        "Obliczenie błędu wynikającego z niepewności analizatora.",
        "Obliczenie statystyk, 95% przedziału ufności, wykres i eksport.",
    ])
    add_equation(doc, "|Z|lin = 10^(|Z|dB/20)")
    add_equation(doc, "Delta Z = |Z|lin · (10^(UD/20) - 1)")

    add_heading(doc, "5.2. Sondy pola bliskiego w analizie emisji promieniowanej", level=2)
    add_steps(doc, [
        "Lista kontrolna stanowiska i zapis temperatury, wilgotności oraz ciśnienia.",
        "Wprowadzenie maksimum mocy dla linii 30 Ω, 50 Ω i 100 Ω w zakresie 100–1000 MHz.",
        "Zastosowanie wzmocnienia K i poprawki sondy Sp oraz obliczenie pola H.",
        "Porównanie charakterystyk, wyznaczenie maksimów, trendów i eksport.",
    ])
    add_equation(doc, "H[dBA/m] = P[dBm] - 30 + 10·log10(50) - K + Sp")
    add_equation(doc, "uH = √(uP² + uK² + uSp²),    U95 = 2·uH")

    add_heading(doc, "6. Interfejs użytkownika")
    add_image(doc, ASSETS / "mapa_interfejsu.png", "Rysunek 3. Stałe elementy interfejsu programu.")
    add_body(doc, "Nagłówek informuje o aktywnym scenariuszu i kroku. Pasek postępu pokazuje pozycję w kreatorze. Środkowy panel zawiera tabelę, wzór lub wykres, natomiast dolny pasek udostępnia nawigację, zmianę scenariusza i eksport. Przycisk Dalej pozostaje nieaktywny do chwili uzupełnienia wymaganych danych.")

    add_heading(doc, "7. Weryfikacja")
    add_table(
        doc,
        ["Test", "Kryterium", "Rezultat"],
        [
            ("Kompilacja Release", "brak błędów i ostrzeżeń", "spełnione"),
            ("Uruchomienie okna", "proces nie kończy się przedwcześnie", "spełnione"),
            ("Wzory przeników", "wartości referencyjne", "spełnione"),
            ("Pole H", "-99,2603 dBA/m dla przykładu 200 MHz", "spełnione"),
            ("Niepewność sond", "U95≈0,7331 dB", "spełnione"),
            ("Nawigacja", "pełne przejście obu kreatorów", "spełnione"),
            ("Eksport", "wymagane sekcje CSV", "spełnione"),
        ],
        [2100, 4700, 2560],
    )

    add_heading(doc, "8. Wieloplatformowość")
    add_body(doc, "Projekt można opublikować jako samowystarczalną paczkę win-x64 lub linux-x64. Rozwiązanie nie wykorzystuje WPF, rejestru Windows ani innych mechanizmów specyficznych dla jednego systemu. Zapis plików odbywa się przez wieloplatformowy interfejs Avalonia StorageProvider.")
    add_callout(doc, "Granica zgodności", "Ze względu na wymagania .NET 8 aplikacja jest przeznaczona dla Windows 10/11. Windows 7 i Windows 8.1 nie są wspieranymi systemami docelowymi.", "warning")

    add_heading(doc, "9. Wartość dydaktyczna")
    add_bullets(doc, [
        "wymuszenie poprawnej kolejności działań laboratoryjnych;",
        "jawna prezentacja wzorów przed pokazaniem wyniku;",
        "oddzielenie danych surowych od danych skorygowanych;",
        "ułatwienie analizy niepewności i porównania serii;",
        "ograniczenie błędów rachunkowych i jednostkowych;",
        "możliwość wykorzystania eksportu jako podstawy tabel w sprawozdaniu.",
    ])

    add_heading(doc, "10. Ograniczenia")
    add_body(doc, "Program nie zastępuje oceny metrologicznej ani instrukcji obsługi aparatury. Domyślne współczynniki i niepewności należy weryfikować dla konkretnego stanowiska. W wersji 1.0 dane nie są odczytywane automatycznie z urządzeń, a sesja nie jest zapisywana w formacie projektu.")

    add_heading(doc, "11. Możliwości dalszego rozwoju")
    add_bullets(doc, [
        "import CSV bezpośrednio z analizatora lub miernika;",
        "interpolacja charakterystyk aparatury z plików kalibracyjnych;",
        "generowanie kompletnego sprawozdania DOCX/PDF;",
        "zapis i ponowne otwieranie sesji pomiarowej;",
        "kolejne scenariusze laboratoryjne;",
        "automatyczne testy interfejsu oraz instalatory.",
    ])

    add_heading(doc, "12. Wnioski")
    add_body(doc, "Zrealizowano wymagany program w języku C# z wieloplatformowym interfejsem graficznym. Kreatory nie ograniczają się do kalkulatora: prowadzą przez przygotowanie pomiaru, kontrolują dane, prezentują podstawę matematyczną i tworzą wynik gotowy do dalszej analizy. Architektura MVVM i wydzielenie usług obliczeniowych umożliwiają rozszerzanie aplikacji bez przebudowy istniejących scenariuszy.")

    path = OUTPUT / "02_Raport_projektowy_dla_prowadzacego.docx"
    doc.save(path)
    return path


def user_manual():
    doc = Document()
    setup_document(doc, "Instrukcja użytkownika", "compact")
    add_cover(
        doc,
        "Instrukcja użytkowania",
        "Obsługa programu EMC Lab Assistant krok po kroku",
        "Podręcznik użytkownika",
        [
            ("Program", PROJECT),
            ("Wersja", VERSION),
            ("Odbiorca", "Student wykonujący ćwiczenie laboratoryjne"),
            ("Scenariusze", "Przeniki mikropaskowe; sondy pola bliskiego"),
            ("System", "Windows 10/11 lub Linux x64"),
            ("Autor", AUTHOR),
        ],
    )

    add_heading(doc, "1. Szybki start")
    add_steps(doc, [
        "Uruchom plik programu odpowiedni dla swojego systemu.",
        "Na ekranie startowym wybierz ćwiczenie.",
        "Wykonuj polecenia widoczne w kolejnych krokach.",
        "Po uzupełnieniu tabeli wybierz Dalej.",
        "Na ostatnim ekranie sprawdź wykres i użyj Eksportuj CSV.",
    ])
    add_callout(doc, "Ważne", "Przycisk Dalej jest nieaktywny, dopóki wszystkie wymagane pola nie są poprawnie uzupełnione.", "info")

    add_heading(doc, "2. Wymagania i uruchomienie")
    add_table(
        doc,
        ["Wariant", "Sposób uruchomienia"],
        [
            ("Paczka samowystarczalna", "Uruchom CrosstalkAnalyzer.exe w Windows albo plik CrosstalkAnalyzer w Linux."),
            ("Kod źródłowy", "W katalogu projektu wykonaj polecenie: dotnet run."),
        ],
        [2700, 6660],
    )
    add_body(doc, "Program zapisuje wyłącznie pliki wybrane przez użytkownika. Nie wymaga połączenia z Internetem i nie wysyła danych pomiarowych.")

    add_heading(doc, "3. Układ okna")
    add_image(doc, ASSETS / "mapa_interfejsu.png", "Rysunek 1. Rozmieszczenie najważniejszych elementów.")
    add_table(
        doc,
        ["Element", "Znaczenie"],
        [
            ("Nagłówek", "Nazwa kroku, scenariusza oraz krótka instrukcja."),
            ("Pasek postępu", "Pozycja od 1 do 4 w bieżącym scenariuszu."),
            ("Obszar roboczy", "Lista kontrolna, tabela danych, wzór, wyniki lub wykres."),
            ("Wstecz", "Powrót do poprzedniego kroku bez usuwania danych."),
            ("Zmień scenariusz", "Powrót do listy ćwiczeń."),
            ("Dalej", "Przeliczenie danych i przejście do kolejnego kroku."),
            ("Eksportuj CSV", "Zapis kompletnego podsumowania na ostatnim ekranie."),
        ],
        [2300, 7060],
    )

    add_heading(doc, "4. Wybór scenariusza")
    add_body(doc, "Ekran startowy zawiera dwie karty. Wybierz scenariusz odpowiadający wykonywanemu ćwiczeniu. Zmiana scenariusza jest możliwa w każdej chwili przyciskiem Zmień scenariusz.")
    add_image(doc, ASSETS / "przeplyw_scenariuszy.png", "Rysunek 2. Kolejność kroków w obu scenariuszach.")

    add_heading(doc, "5. Scenariusz: pomiar przeników")
    add_heading(doc, "Krok 1. Dane wejściowe", level=2)
    add_steps(doc, [
        "Wybierz pasmo 1–2 GHz, 2–3 GHz albo 7–8 GHz.",
        "Program utworzy 11 punktów częstotliwości.",
        "Wpisz przenik bliski NEXT i przenik daleki FEXT w dB.",
        "Używaj wartości od -200 dB do 0 dB.",
        "Opcjonalnie wybierz Wstaw dane przykładowe, aby zaprezentować działanie programu.",
    ])
    add_callout(doc, "Format liczb", "Można wpisywać liczby dziesiętne zgodnie z ustawieniami regionalnymi systemu. Wartości przeników są zwykle ujemne.", "info")

    add_heading(doc, "Krok 2. Konwersja", level=2)
    add_body(doc, "Program pokazuje dane wejściowe i ich odpowiedniki w skali liniowej. Ekran jest tylko do odczytu.")
    add_equation(doc, "|Z|lin = 10^(|Z|dB/20)")

    add_heading(doc, "Krok 3. Błąd analizatora", level=2)
    add_body(doc, "Sprawdź przyjętą wartość UD i przedziały każdego punktu. Wersja 1.0 przyjmuje 0,2 dB dla pasm do 3 GHz i 0,3 dB dla pasma 7–8 GHz.")
    add_equation(doc, "Delta Z = |Z|lin · (10^(UD/20) - 1)")

    add_heading(doc, "Krok 4. Podsumowanie", level=2)
    add_body(doc, "Ekran zawiera statystyki NEXT/FEXT oraz wykres przebiegów. Przedział ufności dotyczy średniej z punktów w wybranym paśmie.")
    add_bullets(doc, [
        "Nowe badanie - czyści dane i wraca do pierwszego kroku.",
        "Eksportuj CSV - zapisuje punkty, błędy, granice oraz statystyki.",
    ])

    add_heading(doc, "6. Scenariusz: sondy pola bliskiego")
    add_heading(doc, "Krok 1. Przygotowanie stanowiska", level=2)
    add_steps(doc, [
        "Połącz sondę R&S H 400-1 przez wzmacniacz z miernikiem mocy.",
        "Ustaw generator na sygnał niemodulowany i moc 10 dBm.",
        "Ustaw w mierniku częstotliwość zgodną z generatorem.",
        "Zapisz temperaturę, wilgotność i ciśnienie.",
        "Zaznacz wszystkie pozycje listy kontrolnej.",
    ])
    add_callout(doc, "Technika pomiaru", "Pętlę sondy przystaw ściśle do kabla. Przesuwaj ją wzdłuż całego kabla i obracaj tak, aby znaleźć maksimum globalne.", "warning")

    add_heading(doc, "Krok 2. Pomiary", level=2)
    add_body(doc, "Dla częstotliwości 100–1000 MHz wpisz moc P w dBm dla linii 30 Ω, 50 Ω i 100 Ω. Kolumny K i Sp są wypełnione wartościami odczytanymi z materiałów ćwiczenia, ale pozostają edytowalne.")
    add_table(
        doc,
        ["Kolumna", "Co wpisać"],
        [
            ("P — 30 Ω", "Maksimum wskazania dla płytki 30 Ω."),
            ("P — 50 Ω", "Maksimum wskazania dla płytki 50 Ω."),
            ("P — 100 Ω", "Maksimum wskazania dla płytki 100 Ω."),
            ("K", "Wzmocnienie toru pomiarowego w dB."),
            ("Sp", "Poprawka sondy pola bliskiego w dB."),
        ],
        [2600, 6760],
    )
    add_callout(doc, "Dane przykładowe", "Przycisk służy do demonstracji i testowania programu. Przed przygotowaniem sprawozdania zastąp te wartości własnymi pomiarami.", "warning")

    add_heading(doc, "Krok 3. Pole H i niepewność", level=2)
    add_equation(doc, "H[dBA/m] = P[dBm] - 30 + 10·log10(50) - K + Sp")
    add_equation(doc, "H[A/m] = 10^(H[dBA/m]/20)")
    add_body(doc, "Program pokazuje wyniki dla każdej impedancji oraz 95% przedziały. Domyślna niepewność rozszerzona wynosi około 0,733 dB.")

    add_heading(doc, "Krok 4. Porównanie", level=2)
    add_body(doc, "Tabela podsumowuje największą wartość H, odpowiadającą częstotliwość oraz oszacowaną szybkość zmian. Wykres umożliwia porównanie linii 30 Ω, 50 Ω i 100 Ω.")
    add_callout(doc, "Interpretacja trendu", "Wartość dodatnia w kolumnie dB/100 MHz oznacza, że w całym badanym zakresie poziom H ma tendencję rosnącą. Lokalne maksima i spadki nadal mogą występować.", "info")

    add_heading(doc, "7. Eksport CSV")
    add_steps(doc, [
        "Przejdź do czwartego kroku.",
        "Wybierz Eksportuj CSV.",
        "Wskaż katalog i nazwę pliku.",
        "Otwórz plik w Excelu, LibreOffice Calc lub innym arkuszu.",
        "W razie potrzeby wybierz separator średnik i kodowanie UTF-8.",
    ])
    add_body(doc, "Eksport sond obejmuje również warunki środowiskowe, wartości K i Sp, pole w dwóch skalach, U95, granice przedziałów, maksima oraz trendy.")

    add_heading(doc, "8. Najczęstsze problemy")
    add_table(
        doc,
        ["Objaw", "Przyczyna", "Rozwiązanie"],
        [
            ("Dalej jest nieaktywne", "Brakuje danych lub pozycja listy kontrolnej nie jest zaznaczona.", "Sprawdź komunikat pod tabelą i wszystkie pola."),
            ("Wartość nie zostaje przyjęta", "Nieprawidłowy format lub zakres liczby.", "Usuń jednostkę z pola; wpisz wyłącznie liczbę."),
            ("Wynik wygląda nietypowo", "Nieprawidłowy znak K/Sp albo jednostka wejścia.", "Sprawdź, czy P jest w dBm, a Sp może być ujemne."),
            ("CSV jest w jednej kolumnie", "Arkusz nie rozpoznał separatora.", "Zaimportuj plik z separatorem średnikowym."),
            ("Nie można zapisać pliku", "Brak uprawnień lub plik jest otwarty.", "Wybierz inny katalog/nazwę i zamknij plik w arkuszu."),
        ],
        [2100, 3400, 3860],
    )

    add_heading(doc, "9. Dobre praktyki")
    add_bullets(doc, [
        "Zapisuj CSV natychmiast po zakończeniu serii pomiarowej.",
        "Nie traktuj danych przykładowych jako wyników laboratoryjnych.",
        "Przed eksportem sprawdź znaki i jednostki wszystkich współczynników.",
        "Zachowuj stałą orientację sondy podczas porównywania serii.",
        "W sprawozdaniu podaj zastosowane wzory i źródła niepewności.",
        "Porównuj wyniki z instrukcją oraz kartą katalogową użytej aparatury.",
    ])

    add_heading(doc, "10. Skrócona lista kontrolna przed oddaniem sprawozdania")
    add_bullets(doc, [
        "Użyto własnych danych pomiarowych.",
        "Wartości K, Sp i UD zostały zweryfikowane.",
        "Tabela zawiera jednostki.",
        "Wykres ma podpisane osie i legendę.",
        "Podano niepewność i sposób jej obliczenia.",
        "Plik CSV został zachowany wraz ze sprawozdaniem.",
    ])

    path = OUTPUT / "03_Instrukcja_uzytkowania_EMC_Lab_Assistant.docx"
    doc.save(path)
    return path


def main():
    create_diagrams()
    paths = [technical_document(), project_report(), user_manual()]
    for path in paths:
        print(path)


if __name__ == "__main__":
    main()
