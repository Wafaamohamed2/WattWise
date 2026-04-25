import os
import glob

directory = r'f:\Energy Optimizer\EnergyOptimizer.API\wwwroot'
for filepath in glob.glob(os.path.join(directory, '*.*')):
    if filepath.endswith('.html') or filepath.endswith('.js'):
        try:
            with open(filepath, 'r', encoding='utf-8') as f:
                content = f.read()
            if '/api/v1/' in content:
                print(f"Skipping {filepath}, already has v1")
                # Wait, what if there are some /api/ and some /api/v1/ ?
            content = content.replace('/api/v1/', '/api/') # remove v1 first if any
            content = content.replace('/api/', '/api/v1/')
            
            with open(filepath, 'w', encoding='utf-8') as f:
                f.write(content)
            print(f"Processed {filepath}")
        except Exception as e:
            print(f"Error {filepath}: {e}")
