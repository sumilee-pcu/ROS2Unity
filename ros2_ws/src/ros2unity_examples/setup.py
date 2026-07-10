from glob import glob
from setuptools import find_packages, setup


package_name = "ros2unity_examples"


setup(
    name=package_name,
    version="0.1.0",
    packages=find_packages(exclude=["test"]),
    data_files=[
        ("share/ament_index/resource_index/packages", [f"resource/{package_name}"]),
        (f"share/{package_name}", ["package.xml"]),
        (f"share/{package_name}/launch", glob("launch/*.launch.py")),
    ],
    install_requires=["setuptools"],
    zip_safe=True,
    maintainer="sumilee",
    maintainer_email="sumilee-pcu@users.noreply.github.com",
    description="ROS 2 Jazzy examples for Unity bridge validation.",
    license="Proprietary",
    entry_points={
        "console_scripts": [
            "bridge_probe = ros2unity_examples.bridge_probe:main",
        ],
    },
)
