import pandas as pd
df = pd.read_excel('Xgear_products_import_draft.xlsx')
with open('temp_excel.md', 'w', encoding='utf-8') as f:
    f.write(df.head().to_markdown())
